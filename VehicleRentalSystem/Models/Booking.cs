using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalSystem.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Upcoming;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ActualReturnDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LateFee { get; set; } = 0;
    }
}