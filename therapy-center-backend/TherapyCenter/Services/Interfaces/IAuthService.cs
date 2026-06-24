using TherapyCenter.DTO_s.Auth;

namespace TherapyCenter.Services.Interfaces
{
    public interface IAuthService
    {
        Task<OtpStartResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> CreateStaffAccountAsync(RegisterRequest request);

        Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);

        Task<OtpStartResponse> ResendOtpAsync(ResendOtpRequest request);
    }
}