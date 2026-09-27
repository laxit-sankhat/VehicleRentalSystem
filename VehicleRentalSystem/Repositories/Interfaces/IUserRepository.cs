using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> AnyAdminExistsAsync();
    }
}