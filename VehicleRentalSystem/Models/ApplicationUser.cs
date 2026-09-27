using Microsoft.AspNetCore.Identity;

namespace VehicleRentalSystem.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;

        public string? DLNumber { get; set; }

        public DateTime? DLExpiryDate { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}