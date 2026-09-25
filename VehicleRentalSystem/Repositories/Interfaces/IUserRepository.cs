using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(User user);
        Task<bool> AnyAdminExistsAsync();
        Task UpdateAsync(User user);
    }
}