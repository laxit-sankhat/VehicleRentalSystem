namespace VehicleRentalSystem.Models
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int ActiveRentals { get; set; }
        public int TotalBookings { get; set; }
        public string? MostBookedVehicleName { get; set; }
        public List<VehicleBookingCount> VehicleBookingCounts { get; set; } = new();
    }

    public class VehicleBookingCount
    {
        public string VehicleName { get; set; } = "";
        public int Count { get; set; }
    }
}