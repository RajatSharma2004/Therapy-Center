namespace TherapyCenter.DTO_s.Patient
{
    public class UpdatePatientRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MedicalHistory { get; set; }

        public int? GuardianId { get; set; }
    }
}
