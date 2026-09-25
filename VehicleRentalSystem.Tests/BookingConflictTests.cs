using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.Implementations;
using Xunit;

namespace VehicleRentalSystem.Tests
{
    public class BookingConflictTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task HasConflictAsync_ReturnsTrue_WhenDatesOverlap()
        {
            // Arrange
            var context = GetInMemoryContext();
            var repo = new BookingRepository(context);

            context.Bookings.Add(new Booking
            {
                VehicleId = 1,
                UserId = 1,
                StartDate = new DateTime(2026, 6, 5),
                EndDate = new DateTime(2026, 6, 10),
                Status = BookingStatus.Upcoming,
                TotalPrice = 1000
            });
            await context.SaveChangesAsync();

            // Act — new booking June 8-12 overlaps with existing June 5-10
            bool result = await repo.HasConflictAsync(1, new DateTime(2026, 6, 8), new DateTime(2026, 6, 12));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasConflictAsync_ReturnsFalse_WhenDatesDontOverlap()
        {
            var context = GetInMemoryContext();
            var repo = new BookingRepository(context);

            context.Bookings.Add(new Booking
            {
                VehicleId = 1,
                UserId = 1,
                StartDate = new DateTime(2026, 6, 5),
                EndDate = new DateTime(2026, 6, 10),
                Status = BookingStatus.Upcoming,
                TotalPrice = 1000
            });
            await context.SaveChangesAsync();

            // Act — new booking June 11-15 does NOT overlap
            bool result = await repo.HasConflictAsync(1, new DateTime(2026, 6, 11), new DateTime(2026, 6, 15));

            Assert.False(result);
        }

        [Fact]
        public async Task HasConflictAsync_ReturnsFalse_WhenExistingBookingIsCancelled()
        {
            var context = GetInMemoryContext();
            var repo = new BookingRepository(context);

            context.Bookings.Add(new Booking
            {
                VehicleId = 1,
                UserId = 1,
                StartDate = new DateTime(2026, 6, 5),
                EndDate = new DateTime(2026, 6, 10),
                Status = BookingStatus.Cancelled, // cancelled — should NOT block
                TotalPrice = 1000
            });
            await context.SaveChangesAsync();

            bool result = await repo.HasConflictAsync(1, new DateTime(2026, 6, 6), new DateTime(2026, 6, 9));

            Assert.False(result);
        }

        [Fact]
        public async Task HasConflictAsync_AllowsBackToBackBooking_SameDayCheckoutCheckin()
        {
            var context = GetInMemoryContext();
            var repo = new BookingRepository(context);

            context.Bookings.Add(new Booking
            {
                VehicleId = 1,
                UserId = 1,
                StartDate = new DateTime(2026, 6, 5),
                EndDate = new DateTime(2026, 6, 10),
                Status = BookingStatus.Upcoming,
                TotalPrice = 1000
            });
            await context.SaveChangesAsync();

            // New booking starts exactly when old one ends — should be ALLOWED
            bool result = await repo.HasConflictAsync(1, new DateTime(2026, 6, 10), new DateTime(2026, 6, 12));

            Assert.False(result);
        }
    }
}