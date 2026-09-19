using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<bool> HasConflictAsync(int vehicleId, DateTime startDate, DateTime endDate);
        Task AddAsync(Booking booking);
        Task<List<Booking>> GetByUserIdAsync(int userId);
        Task<List<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task UpdateAsync(Booking booking);
        Task MarkAsReturnedAsync(int bookingId, decimal lateFee);
        Task UpdateOngoingStatusesAsync();
    }
}