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
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LessonsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Lessons
        public async Task<IActionResult> Index()
        {
            var user = await _context.Users.FindAsync(_userManager.GetUserId(User));  //pobieranie aktualnego zalogowanego użytkownika
            if (user!=null)
            {
                var roles = await _userManager.GetRolesAsync(user);     //pobieranie wszystkich ról aktualnego użytkownika
                if (roles.Any(role => role == "Administrator"))       //sprawdzenie czy użytkownik ma role administratora
                {
                    return View(await _context.Lesson.OrderBy(l => l.DateTimeStarted).ToListAsync());       //widok dla admina - wszystkich zajęć
                }
            }
            return View(await _context.Lesson.Where(m => m.DateTimeStarted > DateTime.Now).OrderBy(m => m.DateTimeStarted).ToListAsync());      //widok dla nie admin - bez przestarzałych zajęć
        }

        // GET: Lessons/Details/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Lesson == null)
            {
                return NotFound();
            }

            var applicationDbContext = _context.MyLessons.Include(u => u.Lessons).Include(u => u.User).Where(m => m.LessonsId == id);       //pobranie wszystkich zapisów na wybrane zajęcia
           
            
            
            var lesson = await _context.Lesson
                .FirstOrDefaultAsync(m => m.Id == id);      //pobranie wybranej lekcji

            ViewData["Lesson"] = lesson;

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Lessons/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Lessons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,Name,DateTimeStarted,DateTimeEnded,AmountOfSpace")] Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lesson);
        }

        // GET: Lessons/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Lesson == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lesson.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DateTimeStarted,DateTimeEnded,AmountOfSpace,AmountOfOccupiedSpace")] Lesson lesson)
        {
            if (id != lesson.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(lesson.Id))
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
            return View(lesson);
        }

        // GET: Lessons/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Lesson == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lesson
                .FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Lesson == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Lesson'  is null.");
            }
            var lesson = await _context.Lesson.FindAsync(id);       //pobranie wybranych zajęć
            if (lesson != null)
            {
                var myLessons = await _context.MyLessons.Where(m => m.LessonsId == id).ToListAsync();       //pobranie wszystkich zapisów na wybrane zajęcia
                foreach (MyLessons myLesson in myLessons)
                {
                    _context.MyLessons.Remove(myLesson);        //usunięcie wszystkich zapisów na wybrane zajęcia
                }
                _context.Lesson.Remove(lesson);     //usunięcie wybranych zajęc
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool LessonExists(int id)
        {
          return _context.Lesson.Any(e => e.Id == id);
        }

    }
}
