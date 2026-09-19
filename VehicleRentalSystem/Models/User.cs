using System.ComponentModel.DataAnnotations;

namespace VehicleRentalSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Phone]
        public string? Phone { get; set; }

        // Nullable because Admin doesn't need these
        public string? DLNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Navigation property: one User can have many Bookings
        public List<Booking> Bookings { get; set; } = new();
    }
}