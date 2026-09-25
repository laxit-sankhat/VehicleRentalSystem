using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.Interfaces;

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