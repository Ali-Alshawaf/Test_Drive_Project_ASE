using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Test_Drive.Models;

namespace Test_Drive.Controllers
{
    public class CarsController : Controller
    {
        private readonly ODB _context;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public CarsController(ODB context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Cars
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Index()
        {
            return View(await _context.Cars.ToListAsync());
        }

        public async Task<IActionResult> IndexForUser()
        {
            return View(await _context.Cars.ToListAsync());
        }

        // GET: Cars/Details/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cars = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cars == null)
            {
                return NotFound();
            }

            return View(cars);
        }


        // GET: Cars/Details/5
        [Authorize(Roles = "User")]

        public async Task<IActionResult> DetailForUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cars = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cars == null)
            {
                return NotFound();
            }

            return View(cars);
        }

        // GET: Cars/Create
        [Authorize(Roles = "Admin")]

        public IActionResult Create()
        {
            return View();
        }

        // POST: Cars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create([Bind("Id,Name,Model,Engine,Hp,Image")] Cars cars, IFormFile Image)
        {
            if (ModelState.IsValid)
            {
                if (Image != null && Image.Length > 0)
                {
                    // Generate a unique filename for the image
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;

                    // Set the path where the image will be saved
                    string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueFileName);

                    // Save the image file to the specified path
                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }

                    // Save the file path to the database
                    cars.Image = "/images/" + uniqueFileName;
                }

                _context.Add(cars);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(cars);
        }

        // GET: Cars/Edit/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cars = await _context.Cars.FindAsync(id);
            if (cars == null)
            {
                return NotFound();
            }
            return View(cars);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Model,Engine,Hp,Image")] Cars cars, IFormFile Image)
        {
            if (id != cars.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCar = await _context.Cars.FindAsync(id);
                    if (existingCar == null)
                    {
                        return NotFound();
                    }

                    if (Image != null && Image.Length > 0)
                    {
                        // Generate a unique filename for the image
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;

                        // Set the path where the image will be saved
                        string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueFileName);

                        // Save the image file to the specified path
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }

                        // Save the new file path to the database
                        existingCar.Image = "/images/" + uniqueFileName;
                    }

                    // Update other properties of the existing car
                    existingCar.Name = cars.Name;
                    existingCar.Model = cars.Model;
                    existingCar.Engine = cars.Engine;
                    existingCar.Hp = cars.Hp;

                    _context.Update(existingCar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarsExists(cars.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cars);
        }

        // Controller action to delete a Cars
        [HttpPost]
        [Authorize(Roles = "Admin")]

        public ActionResult DeleteCars(int id)
        {
            try
            {
                // Get the Cars from the database
                Cars cars = _context.Cars.Find(id);

                // Delete the Cars
                _context.Cars.Remove(cars);
                _context.SaveChanges();

                // Return a success response
                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                // Return an error response
                return Json(new { status = "error", message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        private bool CarsExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}
