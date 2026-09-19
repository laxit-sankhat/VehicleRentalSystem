using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.Interfaces
{
    public interface IVehicleRepository
    {
        Task<List<Vehicle>> GetAllActiveAsync();
        Task<Vehicle?> GetByIdAsync(int id);
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task SoftDeleteAsync(int id);
    }
}