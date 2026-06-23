using TherapyCenter.DTO_s.Admin;
using TherapyCenter.DTO_s.Doctor;
using TherapyCenter.DTO_s.Patient;
using TherapyCenter.Entities;

namespace TherapyCenter.Services.Interfaces
{
    public interface IAdminService
    {
        Task<Therapy> CreateTherapyAsync(CreateTherapyRequest request);
        Task<Therapy> UpdateTherapyAsync(int therapyId, UpdateTherapyRequest request);
        Task DeleteTherapyAsync(int therapyId);
        Task<IEnumerable<Therapy>> GetAllTherapiesAsync();

        Task<DoctorResponse> CreateDoctorProfileAsync(CreateDoctorProfileRequest request);
        Task<IEnumerable<User>> GetAllReceptionistsAsync();
        Task<int> GenerateSlotsForDoctorAsync(GenerateSlotsRequest request);
        Task DeleteDoctorAsync(int doctorId);
        Task DeactivateStaffAsync(int userId);

        Task<IEnumerable<PatientListResponse>> GetAllPatientsAsync();
        Task<PatientListResponse> UpdatePatientAsync(int patientId, UpdatePatientRequest request);
        Task<PatientListResponse?> GetPatientByIdAsync(int patientId);
    }
}
