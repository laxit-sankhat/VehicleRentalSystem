using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.Interfaces;

namespace VehicleRentalSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IBookingRepository _bookingRepository;

        public AdminDashboardController(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<IActionResult> Index()
        {
            await _bookingRepository.UpdateOngoingStatusesAsync();

            var bookings = await _bookingRepository.GetAllAsync();

            var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();

            var vehicleGroups = bookings
                .Where(b => b.Vehicle != null)
                .GroupBy(b => b.Vehicle!.Name)
                .Select(g => new VehicleBookingCount { VehicleName = g.Key, Count = g.Count() })
                .OrderByDescending(v => v.Count)
                .ToList();

            var model = new AdminDashboardViewModel
            {
                TotalRevenue = completedBookings.Sum(b => b.TotalPrice + b.LateFee),
                ActiveRentals = bookings.Count(b => b.Status == BookingStatus.Ongoing),
                TotalBookings = bookings.Count,
                MostBookedVehicleName = vehicleGroups.FirstOrDefault()?.VehicleName ?? "N/A",
                VehicleBookingCounts = vehicleGroups
            };

            return View(model);
        }
    }
}