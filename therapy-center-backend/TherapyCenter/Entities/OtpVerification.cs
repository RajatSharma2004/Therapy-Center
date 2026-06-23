using Microsoft.AspNetCore.Components.Web;

namespace TherapyCenter.Entities
{
    public class OtpVerification
    {
        public int OtpVerificationId { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string Email { get; set; }= string.Empty;
        public string Purpose { get; set; }=string.Empty;
        public string OtpHash { get; set; }=string.Empty;

        public int AttemptCount { get;set; } 

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }


    }
}
