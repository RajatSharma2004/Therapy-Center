namespace TherapyCenter.DTO_s.Patient
{
    public class PatientListResponse
    {
        public int PatientId { get; set; }
        public int? UserId { get; set; }
        public int? GuardianId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MedicalHistory { get; set; }

        public string? GuardianName { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
