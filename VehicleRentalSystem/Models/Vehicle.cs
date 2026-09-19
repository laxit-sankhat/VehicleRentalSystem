using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalSystem.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [Required, MaxLength(100)]
        public string Model { get; set; }

        [Required]
        public VehicleType Type { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Mileage { get; set; }   // km per liter

        public bool HasSunroof { get; set; } = false;

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal PricePerDay { get; set; }

        public string? ImageUrl { get; set; }

        // For "Under Maintenance" toggle (FR-2.4)
        public bool IsUnderMaintenance { get; set; } = false;

        // Soft delete flag (FR-2.3)
        public bool IsActive { get; set; } = true;

        // Navigation property
        public List<Booking> Bookings { get; set; } = new();
    }
}