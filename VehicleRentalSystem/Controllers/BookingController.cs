using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using System.Security.Claims;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.Interfaces;

namespace VehicleRentalSystem.Controllers
{
    [Authorize] // any logged-in user (Customer or Admin)
    public class BookingController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IUserRepository _userRepository;

        public BookingController(
            IBookingRepository bookingRepository,
            IVehicleRepository vehicleRepository,
            IUserRepository userRepository)
        {
            _bookingRepository = bookingRepository;
            _vehicleRepository = vehicleRepository; 
            _userRepository = userRepository;
        }

        // GET: /Booking/Create/5   (5 = vehicleId)
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(int vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null) return NotFound();

            ViewBag.Vehicle = vehicle;
            return View();
        }

        // POST: /Booking/Create
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(int vehicleId, DateTime startDate, DateTime endDate)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);

            // --- Validation 1: dates ---
            if (endDate < startDate)
            {
                ModelState.AddModelError("", "End date must be after start date.");
                ViewBag.Vehicle = vehicle;
                return View();
            }

            // --- Validation 2: age check ---
            if (user!.DateOfBirth.HasValue)
            {
                int age = DateTime.Today.Year - user.DateOfBirth.Value.Year;
                if (user.DateOfBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;

                if (age < 18)
                {
                    ModelState.AddModelError("", "You must be at least 18 years old to book a vehicle.");
                    ViewBag.Vehicle = vehicle;
                    return View();
                }
            }

            // --- Validation 3: DL required, valid format, not expired ---
            if (string.IsNullOrWhiteSpace(user.DLNumber))
            {
                ModelState.AddModelError("", "A valid driving license number is required to book a vehicle.");
                ViewBag.Vehicle = vehicle;
                return View();
            }

            // Indian DL format: 2 letters (state) + 2 digits (RTO) + 4 digit year + 7 digits
            // e.g. GJ01201912345 (with or without hyphens)
            var dlPattern = @"^[A-Za-z]{2}[-\s]?\d{2}[-\s]?\d{4}[-\s]?\d{7}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(user.DLNumber, dlPattern))
            {
                ModelState.AddModelError("", "Your driving license number format appears invalid. Please update your profile.");
                ViewBag.Vehicle = vehicle;
                return View();
            }

            if (!user.DLExpiryDate.HasValue || user.DLExpiryDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("", "Your driving license has expired or expiry date is missing. Please update your profile.");
                ViewBag.Vehicle = vehicle;
                return View();
            }

            // --- Validation 4: vehicle status ---
            if (!vehicle.IsActive || vehicle.IsUnderMaintenance)
            {
                ModelState.AddModelError("", "This vehicle is currently not available for booking.");
                ViewBag.Vehicle = vehicle;
                return View();
            }

            // --- Validation 5: double-booking check ---
            bool conflict = await _bookingRepository.HasConflictAsync(vehicleId, startDate, endDate);
            if (conflict)
            {
                ModelState.AddModelError("", "This vehicle is already booked for the selected dates.");
                ViewBag.Vehicle = vehicle;
                return View();
            }

            // --- Price calculation ---
            int totalDays = (endDate.Date - startDate.Date).Days;
            if (totalDays == 0) totalDays = 1; // same-day rental = minimum 1 day charge

            decimal totalPrice = totalDays * vehicle.PricePerDay;

            var booking = new Booking
            {
                UserId = userId,
                VehicleId = vehicleId,
                StartDate = startDate,
                EndDate = endDate,
                TotalPrice = totalPrice,
                Status = BookingStatus.Upcoming
            };

            await _bookingRepository.AddAsync(booking);

            return RedirectToAction("MyBookings");
        }

        // GET: /Booking/MyBookings
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyBookings()
        {
            await _bookingRepository.UpdateOngoingStatusesAsync();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            return View(bookings);
        }

        // POST: /Booking/Cancel/5
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            {
                TempData["Error"] = "This booking can no longer be cancelled.";
                return RedirectToAction("MyBookings");
            }

            bool withinGracePeriod = DateTime.Now <= booking.CreatedAt.AddMinutes(15);
            bool outside24HourWindow = booking.StartDate > DateTime.Now.AddHours(24);

            if (!withinGracePeriod && !outside24HourWindow)
            {
                TempData["Error"] = "Cannot cancel within 24 hours of rental start.";
                return RedirectToAction("MyBookings");
            }

            booking.Status = BookingStatus.Cancelled;
            await _bookingRepository.UpdateAsync(booking);

            return RedirectToAction("MyBookings");
        }

        // GET: /Booking/AllBookings (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllBookings()
        {
            await _bookingRepository.UpdateOngoingStatusesAsync();

            var bookings = await _bookingRepository.GetAllAsync();
            return View(bookings);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsReturned(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            var actualReturn = DateTime.Now;
            decimal lateFee = 0;

            // end-of-day cutoff — late only if returned on a LATER calendar date
            if (actualReturn.Date > booking.EndDate.Date)
            {
                int lateDays = (actualReturn.Date - booking.EndDate.Date).Days;
                lateFee = lateDays * (booking.Vehicle!.PricePerDay * 1.5m);
            }

            await _bookingRepository.MarkAsReturnedAsync(id, lateFee);

            return RedirectToAction("AllBookings");
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            // Security: a Customer can only view their OWN booking
            if (User.IsInRole("Customer"))
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (booking.UserId != userId)
                    return Forbid();
            }

            return View(booking);
        }

        [Authorize]
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null || booking.Status != BookingStatus.Completed) return NotFound();

            if (User.IsInRole("Customer"))
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (booking.UserId != userId) return Forbid();
            }

            var pdfBytes = GenerateInvoicePdf(booking);
            return File(pdfBytes, "application/pdf", $"Invoice_Booking{booking.Id}.pdf");
        }

        private byte[] GenerateInvoicePdf(Booking booking)
        {
            using var stream = new MemoryStream();

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Header().Text("Vehicle Rental System - Invoice")
                        .FontSize(20).Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Invoice for Booking #{booking.Id}").FontSize(14).Bold();
                        col.Item().Text($"Vehicle: {booking.Vehicle?.Name} ({booking.Vehicle?.Model})");
                        col.Item().Text($"Rental Period: {booking.StartDate:dd MMM yyyy} to {booking.EndDate:dd MMM yyyy}");
                        col.Item().Text($"Actual Return: {booking.ActualReturnDate:dd MMM yyyy hh:mm tt}");

                        col.Item().PaddingTop(15).LineHorizontal(1);

                        col.Item().Text($"Base Rental Price: ₹{booking.TotalPrice}");
                        col.Item().Text($"Late Fee: ₹{booking.LateFee}");
                        col.Item().Text($"Total Paid: ₹{booking.TotalPrice + booking.LateFee}")
                            .FontSize(14).Bold();

                        col.Item().PaddingTop(20).Text("Thank you for choosing our service!")
                            .Italic();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on ").FontSize(9);
                        x.Span(DateTime.Now.ToString("dd MMM yyyy")).FontSize(9);
                    });
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }
    }
}