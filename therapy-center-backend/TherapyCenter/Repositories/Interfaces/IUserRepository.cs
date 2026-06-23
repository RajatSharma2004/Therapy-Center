using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailIncludingInactiveAsync(string email);

        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByIdIncludingInactiveAsync(int userId);

        Task<IEnumerable<User>> GetByRoleAsync(string role);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> EmailExistsVerifiedAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
    }
}
