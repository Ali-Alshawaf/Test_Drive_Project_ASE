using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Test_Drive.Models;

namespace Test_Drive.Controllers
{
    public class BooksController : Controller
    {
        private readonly ODB _context;

        public BooksController(ODB context)
        {
            _context = context;
        }

        // GET: Books
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Index()
        {
            var oDB = _context.Book.Include(b => b.Cars).Include(b => b.Users);
            return View(await oDB.ToListAsync());
        }

        [Authorize(Roles = "User")]

        public async Task<IActionResult> IndexForUser()
        {
            var oDB = _context.Book.Include(b => b.Cars).Include(b => b.Users).Where(u => u.UsersId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            return View(await oDB.ToListAsync());
        }


        // GET: Books/Details/5
        [Authorize(Roles = "User")]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Cars)
                .Include(b => b.Users)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Details/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DetailForAdmin(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Cars)
                .Include(b => b.Users)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "User")]

        public IActionResult Create()
        {
            ViewData["CarsId"] = new SelectList(_context.Cars, "Id", "Id");
            ViewData["UsersId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]

        public async Task<IActionResult> Create([Bind("City,Date")] Book book, int? Id)
        {
            if (book.Date < DateTime.Today)
            {
                ModelState.AddModelError("Date", "Cannot create a book with a past date!");
                return View(book);
            }
            if (_context.Book.Include(s => s.Cars).Any(s => s.Date == book.Date && s.CarsId == Id.Value && s.City == book.City))
            {
                ModelState.AddModelError("Date", "You cannot book at this time. Please choose a different date, car, or city.");
                return View(book);
            }
            if (ModelState.IsValid)
            {
                book.Time = DateTime.Now;
                book.CarsId = Id.Value;
                book.UsersId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexForUser));
            }
            ViewData["CarsId"] = new SelectList(_context.Cars, "Id", "Id", book.CarsId);
            ViewData["UsersId"] = new SelectList(_context.Users, "Id", "Id", book.UsersId);
            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["CarsId"] = new SelectList(_context.Cars, "Id", "Id", book.CarsId);
            ViewData["UsersId"] = new SelectList(_context.Users, "Id", "Id", book.UsersId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,City,Date,Time,CarsId,UsersId")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
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
            ViewData["CarsId"] = new SelectList(_context.Cars, "Id", "Id", book.CarsId);
            ViewData["UsersId"] = new SelectList(_context.Users, "Id", "Id", book.UsersId);
            return View(book);
        }


        // Controller action to delete a booking
        [HttpPost]
        [Authorize(Roles = "Admin,User")]

        public ActionResult DeleteBooking(int id)
        {
            try
            {
                // Get the booking from the database
                Book booking = _context.Book.Find(id);

                // Delete the booking
                _context.Book.Remove(booking);
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



        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.Id == id);
        }
    }
}
