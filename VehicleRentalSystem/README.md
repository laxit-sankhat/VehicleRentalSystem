# Vehicle Rental System

A full-stack vehicle rental management system built with ASP.NET Core MVC, Entity Framework Core, and SQLite. Supports role-based access for Customers and Admins, complete booking lifecycle management, and automated conflict detection.

## Features

- **Authentication & Authorization** — Cookie-based auth with role-based access (Customer/Admin)
- **Vehicle Management** — Full CRUD with soft delete, type-specific validation (Car/Bike/SUV/Van), unique registration numbers
- **Booking System**
  - Real-time date-overlap conflict detection
  - Automatic price calculation
  - 15-minute grace-period cancellation + 24-hour cancellation policy
  - Booking extension with conflict re-validation
  - Late return fee calculation
- **Admin Dashboard** — Revenue, active rentals, and booking analytics with charts
- **PDF Invoice Generation** — Downloadable invoices for completed bookings via QuestPDF
- **Driving License Validation** — Format and expiry checks at booking time
- **Repository Pattern** — Full separation of data access from business logic
- **Automated Testing** — xUnit test suite covering critical booking conflict logic

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core |
| Database | SQLite |
| Auth | Cookie Authentication |
| PDF Generation | QuestPDF |
| Testing | xUnit + Moq + EF Core InMemory |

## Getting Started

### Prerequisites
- .NET SDK 8.0+ (developed on 10.0.401)
- Visual Studio 2022 (or VS Code)

### Setup
```bash
git clone https://github.com/laxit-sankhat/VehicleRentalSystem.git
cd VehicleRentalSystem
dotnet restore
```

Update the database:
```bash
dotnet ef database update
```

Run the project:
```bash
dotnet run
```

A default Admin account is seeded automatically on first run:
- **Email:** admin@rental.com
- **Password:** (see `Program.cs` seed logic)

### Running Tests
```bash
dotnet test
```

## Project Structure

VehicleRentalSystem/
├── Controllers/
├── Models/
├── Repositories/
│ ├── Interfaces/
│ └── Implementations/
├── Views/
├── Data/
└── Migrations/

VehicleRentalSystem.Tests/


## Author

Laxit Sankhat, Urva Patel — college project