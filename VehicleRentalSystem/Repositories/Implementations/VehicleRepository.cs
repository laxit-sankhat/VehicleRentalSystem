using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.Interfaces;

namespace VehicleRentalSystem.Repositories.Implementations
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetAllActiveAsync()
        {
            return await _context.Vehicles.Where(v => v.IsActive).ToListAsync();
        }

        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            return await _context.Vehicles.FindAsync(id);
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                vehicle.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Vehicle>> SearchAsync(VehicleType? type, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Vehicles
                .Where(v => v.IsActive && !v.IsUnderMaintenance)
                .AsQueryable();

            if (type.HasValue)
                query = query.Where(v => v.Type == type.Value);

            if (minPrice.HasValue)
                query = query.Where(v => v.PricePerDay >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(v => v.PricePerDay <= maxPrice.Value);

            return await query.ToListAsync();
        }
    }
}