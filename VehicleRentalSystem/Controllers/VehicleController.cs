using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Vehicle
        public async Task<IActionResult> Index()
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.IsActive)   // hide soft-deleted vehicles
                .ToListAsync();

            return View(vehicles);
        }

        // GET: /Vehicle/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Vehicle/Create
        [HttpPost]
        public async Task<IActionResult> Create(Vehicle model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Vehicles.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: /Vehicle/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // POST: /Vehicle/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Vehicle model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            _context.Vehicles.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // POST: /Vehicle/Delete/5  (soft delete)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            vehicle.IsActive = false;   // soft delete, not real removal
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}