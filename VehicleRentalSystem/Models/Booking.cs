using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleRentalSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
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
    }
}