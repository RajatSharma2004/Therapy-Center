using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IOtpVerificationRepository
    {
        Task<OtpVerification> CreateAsync(OtpVerification otpVerification);
        Task<OtpVerification> UpdateAsync(OtpVerification otpVerification);

        Task<OtpVerification?> GetLatestPendingAsync(int userId, string purpose);
        Task InvalidatePendingAsync(int userId, string purpose);
    }
}
