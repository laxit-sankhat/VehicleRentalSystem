namespace VehicleRentalSystem.Models
{
    public enum UserRole
    {
        Customer,
        Admin
    }

    public enum VehicleType
    {
        Car,
        Bike
    }

    public enum BookingStatus
    {
        Upcoming,
        Ongoing,
        Completed,
        Cancelled
    }
}