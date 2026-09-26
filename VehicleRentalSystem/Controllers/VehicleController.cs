using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.Interfaces;
using System;
using System.Linq;

namespace VehicleRentalSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehicleController : Controller
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleController(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleRepository.GetAllActiveAsync();
            return View(vehicles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Vehicle vehicle)
        {
            if (!ModelState.IsValid)
                return View(vehicle);

            // Custom validation: RegistrationNumber uniqueness
            if (await _vehicleRepository.RegistrationNumberExistsAsync(vehicle.RegistrationNumber, vehicle.Id))
            {
                ModelState.AddModelError(nameof(vehicle.RegistrationNumber), "Registration number must be unique.");
            }

            // Manufacturing year validation
            var currentYear = DateTime.Now.Year;
            if (vehicle.ManufacturingYear < 1990 || vehicle.ManufacturingYear > currentYear)
            {
                ModelState.AddModelError(nameof(vehicle.ManufacturingYear), $"Manufacturing year must be between 1990 and {currentYear}.");
            }

            // Vehicle type specific capacity rules
            switch (vehicle.Type)
            {
                case VehicleType.Car:
                    if (vehicle.Capacity < 1 || vehicle.Capacity > 7)
                        ModelState.AddModelError(nameof(vehicle.Capacity), "For Car, capacity must be between 1 and 7.");
                    break;
                case VehicleType.Bike:
                    if (vehicle.Capacity < 1 || vehicle.Capacity > 2)
                        ModelState.AddModelError(nameof(vehicle.Capacity), "For Bike, capacity must be between 1 and 2.");
                    break;
                case VehicleType.SUV:
                    if (vehicle.Capacity < 1 || vehicle.Capacity > 8)
                        ModelState.AddModelError(nameof(vehicle.Capacity), "For SUV, capacity must be between 1 and 8.");
                    break;
                case VehicleType.Van:
                    if (vehicle.Capacity < 1 || vehicle.Capacity > 20)
                        ModelState.AddModelError(nameof(vehicle.Capacity), "For Van, capacity must be between 1 and 20.");
                    break;
            }

            // FuelType and Transmission validation
            if (!Enum.IsDefined(typeof(FuelType), vehicle.FuelType))
            {
                ModelState.AddModelError(nameof(vehicle.FuelType), "Invalid fuel type.");
            }

            if (!Enum.IsDefined(typeof(TransmissionType), vehicle.Transmission))
            {
                ModelState.AddModelError(nameof(vehicle.Transmission), "Invalid transmission type.");
            }

            if (!ModelState.IsValid)
                return View(vehicle);

            await _vehicleRepository.AddAsync(vehicle);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(vehicle);

            // Custom validation: RegistrationNumber uniqueness (allow own)
            if (await _vehicleRepository.RegistrationNumberExistsAsync(vehicle.RegistrationNumber, vehicle.Id))
            {
                ModelState.AddModelError(nameof(vehicle.RegistrationNumber), "Registration number must be unique.");
            }

            // Manufacturing year validation
            var currentYear = DateTime.Now.Year;
            if (vehicle.ManufacturingYear < 1990 || vehicle.ManufacturingYear > currentYear)
            {
                ModelState.AddModelError(nameof(vehicle.ManufacturingYear), $"Manufacturing year must be between 1990 and {currentYear}.");
            }

            // Vehicle type specific capacity rules
            switch (vehicle.Type)
            {
                case VehicleType.Car:
                    if (vehicle.Capacity < 1 || vehicle.Capacity > 7)
                        ModelState.AddModelError(nameof(vehicle.Capacity), "For Car, capacity must be between 1 and 7.");
                    break;
                case VehicleType.Bike:
                    if (vehicle.Capacity < 1 || vehicle.Capacity > 2)
                        ModelState.AddModelError(nameof(vehicle.Capacity), "For Bike, capacity must be between 1 and 2.");
                    break;
                case VehicleType.SUV:
                    if (vehicle.Capacity < 1 || vehicle.Capacity > 8)
                        ModelState.AddModelError(nameof(vehicle.Capacity), "For SUV, capacity must be between 1 and 8.");
                    break;
                case VehicleType.Van:
                    if (vehicle.Capacity < 1 || vehicle.Capacity > 20)
                        ModelState.AddModelError(nameof(vehicle.Capacity), "For Van, capacity must be between 1 and 20.");
                    break;
            }

            // FuelType and Transmission validation
            if (!Enum.IsDefined(typeof(FuelType), vehicle.FuelType))
            {
                ModelState.AddModelError(nameof(vehicle.FuelType), "Invalid fuel type.");
            }

            if (!Enum.IsDefined(typeof(TransmissionType), vehicle.Transmission))
            {
                ModelState.AddModelError(nameof(vehicle.Transmission), "Invalid transmission type.");
            }

            if (!ModelState.IsValid)
                return View(vehicle);

            await _vehicleRepository.UpdateAsync(vehicle);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _vehicleRepository.SoftDeleteAsync(id);
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null || !vehicle.IsActive) return NotFound();

            return View(vehicle);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Browse(VehicleType? type, decimal? minPrice, decimal? maxPrice)
        {
            var vehicles = await _vehicleRepository.SearchAsync(type, minPrice, maxPrice);

            ViewBag.SelectedType = type;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(vehicles);
        }
    }
}