using Microsoft.EntityFrameworkCore;
using TherapyCenter.Data;
using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;

namespace TherapyCenter.Repositories.Implementations
{
    public class OtpVerificationRepository:IOtpVerificationRepository
    {
        private readonly AppDbContext _context;

        public OtpVerificationRepository(AppDbContext context)
        {    _context = context;
        }

        public async Task<OtpVerification> CreateAsync(OtpVerification otpVerification)
        {
            _context.OtpVerifications.Add(otpVerification);
            await _context.SaveChangesAsync();
            return otpVerification;
        }

        public async Task<OtpVerification> UpdateAsync(OtpVerification otpVerification)
        {
            _context.OtpVerifications.Update(otpVerification);
            await _context.SaveChangesAsync();
            return otpVerification;
        }

        public async Task<OtpVerification?> GetLatestPendingAsync(int userId, string purpose)
        {
            return await _context.OtpVerifications
                .Where(x => x.UserId == userId && x.Purpose == purpose && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidatePendingAsync(int userId, string purpose)
        {
            var activeOtps = await _context.OtpVerifications
                .Where(x => x.UserId == userId && x.Purpose == purpose && !x.IsUsed)
                .ToListAsync();

            foreach (var otp in activeOtps)
            {
                otp.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
