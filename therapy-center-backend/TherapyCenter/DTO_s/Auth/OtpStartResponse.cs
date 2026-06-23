namespace TherapyCenter.DTO_s.Auth
{
    public class OtpStartResponse
    {
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
