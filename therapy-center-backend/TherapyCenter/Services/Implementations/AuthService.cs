using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TherapyCenter.DTO_s.Auth;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IOtpVerificationRepository _otpRepo;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher = new();

        private static readonly HashSet<string> SelfRegisterRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Patient",
            "Guardian"
        };

        private static readonly HashSet<string> AllowedOtpRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Patient",
            "Guardian",
            "Receptionist",
            "Doctor"
        };

        public AuthService(
            IUserRepository userRepo,
            IPatientRepository patientRepo,
            IOtpVerificationRepository otpRepo,
            IEmailService emailService,
            IConfiguration config)
        {
            _userRepo = userRepo;
            _patientRepo = patientRepo;
            _otpRepo = otpRepo;
            _emailService = emailService;
            _config = config;
        }

        public async Task<OtpStartResponse> RegisterAsync(RegisterRequest request)
        {
            if (!SelfRegisterRoles.Contains(request.Role))
                throw new InvalidOperationException("Only Patient or Guardian roles can self-register.");

            return await CreatePendingUserAsync(request, "Register");
        }

        public async Task<AuthResponse> CreateStaffAccountAsync(RegisterRequest request)
        {
            if (!AllowedOtpRoles.Contains(request.Role))
                throw new InvalidOperationException("Invalid role for staff creation.");

            var existingUser = await _userRepo.GetByEmailIncludingInactiveAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName  = request.LastName.Trim(),
                Email     = request.Email.Trim(),
                Role      = request.Role.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                IsActive        = true,   // immediately active — no OTP needed
                IsEmailVerified = true
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            await _userRepo.CreateAsync(user);

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByEmailIncludingInactiveAsync(request.Email)
                       ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsEmailVerified)
                throw new UnauthorizedAccessException("Please verify your email first.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been deactivated.");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (string.Equals(user.Role?.Trim(), "Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = await _patientRepo.GetByUserIdAsync(user.UserId);
                if (patient == null)
                {
                    await _patientRepo.CreateAsync(new Patient
                    {
                        UserId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    });
                }
            }

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _userRepo.GetByEmailIncludingInactiveAsync(request.Email)
                       ?? throw new InvalidOperationException("User not found.");

            var otpRecord = await _otpRepo.GetLatestPendingAsync(user.UserId, request.Purpose);

            if (otpRecord == null)
                throw new InvalidOperationException("OTP not found. Please request a new one.");

            if (otpRecord.IsUsed)
                throw new InvalidOperationException("OTP already used. Please request a new one.");

            if (otpRecord.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("OTP has expired. Please request a new one.");

            if (otpRecord.AttemptCount >= 5)
                throw new InvalidOperationException("OTP attempts exceeded. Please request a new one.");

            var inputHash = HashOtp(request.Otp.Trim());

            if (!string.Equals(inputHash, otpRecord.OtpHash, StringComparison.Ordinal))
            {
                otpRecord.AttemptCount++;

                if (otpRecord.AttemptCount >= 5)
                    otpRecord.IsUsed = true;

                await _otpRepo.UpdateAsync(otpRecord);

                throw new InvalidOperationException("Invalid OTP.");
            }

            otpRecord.IsUsed = true;
            await _otpRepo.UpdateAsync(otpRecord);

            user.IsEmailVerified = true;
            user.IsActive = true;
            await _userRepo.UpdateAsync(user);

            if (string.Equals(user.Role, "Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patient = await _patientRepo.GetByUserIdAsync(user.UserId);
                if (patient == null)
                {
                    await _patientRepo.CreateAsync(new Patient
                    {
                        UserId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    });
                }
            }

            return BuildAuthResponse(user);
        }

        public async Task<OtpStartResponse> ResendOtpAsync(ResendOtpRequest request)
        {
            var user = await _userRepo.GetByEmailIncludingInactiveAsync(request.Email)
                       ?? throw new InvalidOperationException("User not found.");

            if (user.IsEmailVerified)
                throw new InvalidOperationException("Account is already verified.");

            await _otpRepo.InvalidatePendingAsync(user.UserId, request.Purpose);

            var otp = GenerateOtp();
            var otpRecord = new OtpVerification
            {
                UserId = user.UserId,
                Email = user.Email,
                Purpose = request.Purpose,
                OtpHash = HashOtp(otp),
                AttemptCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            await _otpRepo.CreateAsync(otpRecord);
            await _emailService.SendOtpAsync(user.Email, otp, request.Purpose);

            return new OtpStartResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Purpose = request.Purpose,
                Message = "OTP resent successfully.",
                ExpiresAt = otpRecord.ExpiresAt
            };
        }

        private async Task<OtpStartResponse> CreatePendingUserAsync(RegisterRequest request, string purpose)
        {
            // Check if email belongs to an existing account
            var existingUser = await _userRepo.GetByEmailIncludingInactiveAsync(request.Email);

            if (existingUser != null)
            {
                // Verified accounts cannot register again
                if (existingUser.IsEmailVerified)
                    throw new InvalidOperationException("Email is already registered.");

                // Pending account exists.
                // Update it with the latest registration details instead of creating a duplicate.
                existingUser.FirstName = request.FirstName.Trim();
                existingUser.LastName = request.LastName.Trim();
                existingUser.PhoneNumber = request.PhoneNumber?.Trim();

                existingUser.PasswordHash = _hasher.HashPassword(existingUser, request.Password);

                await _userRepo.UpdateAsync(existingUser);

                // Keep patient profile in sync with the latest registration data
                if (string.Equals(existingUser.Role, "Patient", StringComparison.OrdinalIgnoreCase))
                {
                    var patient = await _patientRepo.GetByUserIdAsync(existingUser.UserId);
                    if (patient != null)
                    {
                        patient.Gender = request.Gender;
                        patient.DateOfBirth = request.DateOfBirth;
                       

                        await _patientRepo.UpdateAsync(patient);
                    }
                    else
                    {
                        await _patientRepo.CreateAsync(new Patient
                        {
                            UserId = existingUser.UserId,
                            FirstName = existingUser.FirstName,
                            LastName = existingUser.LastName,
                            Gender = request.Gender,
                            DateOfBirth = request.DateOfBirth,
                           
                        });
                    }
                }

                // Previous OTPs should no longer be valid once details are updated
                await _otpRepo.InvalidatePendingAsync(existingUser.UserId, purpose);

                var otp = GenerateOtp();
                var otpRecord = new OtpVerification
                {
                    UserId = existingUser.UserId,
                    Email = existingUser.Email,
                    Purpose = purpose,
                    OtpHash = HashOtp(otp),
                    AttemptCount = 0,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };

                await _otpRepo.CreateAsync(otpRecord);
                await _emailService.SendOtpAsync(existingUser.Email, otp, purpose);

                return new OtpStartResponse
                {
                    UserId = existingUser.UserId,
                    Email = existingUser.Email,
                    Purpose = purpose,
                    Message = "Verification OTP resent.",
                    ExpiresAt = otpRecord.ExpiresAt
                };
            }

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim(),
                Role = request.Role.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                IsActive = false,
                IsEmailVerified = false
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            await _userRepo.CreateAsync(user);

            if (string.Equals(request.Role, "Patient", StringComparison.OrdinalIgnoreCase))
            {
                await _patientRepo.CreateAsync(new Patient
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                   
                });
            }

            var newOtp = GenerateOtp();
            var newOtpRecord = new OtpVerification
            {
                UserId = user.UserId,
                Email = user.Email,
                Purpose = purpose,
                OtpHash = HashOtp(newOtp),
                AttemptCount = 0,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            await _otpRepo.InvalidatePendingAsync(user.UserId, purpose);
            await _otpRepo.CreateAsync(newOtpRecord);
            await _emailService.SendOtpAsync(user.Email, newOtp, purpose);

            return new OtpStartResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Purpose = purpose,
                Message = "OTP sent successfully. Please verify to activate your account.",
                ExpiresAt = newOtpRecord.ExpiresAt
            };
        }

        private AuthResponse BuildAuthResponse(User user)
        {
            return new AuthResponse
            {
                Token = GenerateToken(user),
                Role = user.Role,
                UserId = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }

        private string GenerateToken(User user)
        {
            var jwt = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateOtp()
        {
            var value = RandomNumberGenerator.GetInt32(100000, 999999);
            return value.ToString();
        }

        private static string HashOtp(string otp)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToHexString(bytes);
        }
    }
}