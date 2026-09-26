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
        Bike,
        SUV,
        Van
    }

    public enum BookingStatus
    {
        Upcoming,
        Ongoing,
        Completed,
        Cancelled
    }

    public enum FuelType
    {
        Petrol,
        Diesel,
        Electric,
        Hybrid
    }

    public enum TransmissionType
    {
        Manual,
        Automatic,
        CVT
    }
}