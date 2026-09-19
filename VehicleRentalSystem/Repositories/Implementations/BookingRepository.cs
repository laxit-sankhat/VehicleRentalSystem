using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.Interfaces;

namespace VehicleRentalSystem.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasConflictAsync(int vehicleId, DateTime startDate, DateTime endDate)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.VehicleId == vehicleId &&
                b.Status != BookingStatus.Cancelled &&
                b.StartDate < endDate &&
                b.EndDate > startDate);
        }

        public async Task AddAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Booking>> GetByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReturnedAsync(int bookingId, decimal lateFee)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                booking.ActualReturnDate = DateTime.Now;
                booking.LateFee = lateFee;
                booking.Status = BookingStatus.Completed;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateOngoingStatusesAsync()
        {
            var bookingsToUpdate = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Upcoming && b.StartDate <= DateTime.Now)
                .ToListAsync();

            foreach (var booking in bookingsToUpdate)
            {
                booking.Status = BookingStatus.Ongoing;
            }

            if (bookingsToUpdate.Any())
                await _context.SaveChangesAsync();
        }
    }
}