using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace VehicleRentalSystem.Models
{
    [Index(nameof(RegistrationNumber), IsUnique = true)]
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [Required, StringLength(100, MinimumLength = 1)]
        public string Model { get; set; }

        [Required, StringLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Range(1990, int.MaxValue)]
        public int ManufacturingYear { get; set; }

        [Required]
        public VehicleType Type { get; set; }

        [Range(1, 20)]
        public int Capacity { get; set; }

        [Range(0.1, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Mileage { get; set; }   // km per liter

        public bool HasSunroof { get; set; } = false;

        public FuelType FuelType { get; set; }

        public TransmissionType Transmission { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        [Range(1, 100000)]
        public decimal PricePerDay { get; set; }

        [StringLength(500), Url]
        public string? ImageUrl { get; set; }

        // For "Under Maintenance" toggle (FR-2.4)
        public bool IsUnderMaintenance { get; set; } = false;

        // Soft delete flag (FR-2.3)
        public bool IsActive { get; set; } = true;

        // Navigation property
        public List<Booking> Bookings { get; set; } = new();
    }

    // FuelType and TransmissionType are defined in Models/Enums.cs
}