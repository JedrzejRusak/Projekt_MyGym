using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projekt.Data;
using Projekt.Models;

namespace Projekt.Controllers
{
    [Authorize] // autoryzacja - użytkownik musi być zalogowany
    public class MyLessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public MyLessonsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MyLessons
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MyLessons.Include(m => m.Lessons).Where(m => m.UserId == _userManager.GetUserId(User)).Where(m => m.Lessons.DateTimeStarted > DateTime.Now).OrderBy(m => m.Lessons.DateTimeStarted);        //zapisy na zajęcia zalogwanego użytkownika bez przestarzałych zajęć
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MyLessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MyLessons == null)
            {
                return NotFound();
            }

            var myLessons = await _context.MyLessons
                .Include(m => m.Lessons)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myLessons == null)
            {
                return NotFound();
            }

            return View(myLessons);
        }

        // GET: MyLessons/Create
        public IActionResult Create()
        {
            var userLessons = _context.MyLessons.Include(m => m.Lessons).Where(m => m.UserId == _userManager.GetUserId(User));      //pobranie wszytskich zajęć zalogowanego urzytkownika
            var lessons = _context.Lesson.Where(l => l.AmountOfOccupiedSpace < l.AmountOfSpace).Where(l => l.DateTimeStarted > DateTime.Now).Where(l => !(userLessons.Any(m=> m.Lessons.Id == l.Id)));      //pobranie zajęć dla których są wolne miejsca,które się jeszcze nie odbyły, na które jeszcze nie jest się zapisanym

            ViewData["LessonsId"] = new SelectList(lessons, "Id", "Name");      //przekazanie zajęć do widoku wyboru
            ViewData["AreVacancies"] = lessons.Any();       //przekazanie wartości bool czy są jakieś zajęcia do wyboru
            return View();
        }

        // POST: MyLessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LessonsId")] MyLessons myLessons)
        {
            myLessons.UserId = _userManager.GetUserId(User);
            
            _context.Lesson.Where(l => l.Id == myLessons.LessonsId).First().AmountOfOccupiedSpace += 1;     //zwiększenie liczby zajętych miejsc
            _context.Add(myLessons);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            
            ViewData["LessonsId"] = new SelectList(_context.Lesson, "Id", "Id", myLessons.LessonsId);
            return View(myLessons);
        }

        

        // GET: MyLessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MyLessons == null)
            {
                return NotFound();
            }

            var myLessons = await _context.MyLessons
                .Include(m => m.Lessons)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (myLessons == null)
            {
                return NotFound();
            }

            return View(myLessons);
        }

        // POST: MyLessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MyLessons == null)
            {
                return Problem("Entity set 'ApplicationDbContext.MyLessons'  is null.");
            }
            var myLessons = await _context.MyLessons.FindAsync(id);
            if (myLessons != null)
            {
                _context.Lesson.Where(l => l.Id == myLessons.LessonsId).First().AmountOfOccupiedSpace -= 1;     //zmieniejszenie ilości zajętych miejsc o 1 dla zajęć dla których usuwamy rezerwacje / zapisa na nie 
                _context.MyLessons.Remove(myLessons);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MyLessonsExists(int id)
        {
          return _context.MyLessons.Any(e => e.Id == id);
        }
    }
}
