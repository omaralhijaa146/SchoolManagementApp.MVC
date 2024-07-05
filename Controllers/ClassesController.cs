using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementApp.MVC.Data;
using SchoolManagementApp.MVC.Models;

namespace SchoolManagementApp.MVC.Controllers
{
    public class ClassesController : Controller
    {
        private readonly SchoolManagementDbContext _context;
        private readonly INotyfService _notyfService;

        public ClassesController(SchoolManagementDbContext context, INotyfService notyfService)
        {
            _context = context;
            this._notyfService = notyfService;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            var schoolManagementDbContext = _context.Classes.Include(x => x.Course).Include(x => x.Lecturer);
            return View(await schoolManagementDbContext.ToListAsync());
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(x => x.Course)
                .Include(x => x.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            CreateSelectLists();
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LecturerId,CourseId,Time")] Class @class)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CreateSelectLists();
            return View(@class);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }
            CreateSelectLists();
            return View(@class);
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LecturerId,CourseId,Time")] Class @class)
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
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
            CreateSelectLists();
            return View(@class);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(x => x.Course)
                .Include(x => x.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                _context.Classes.Remove(@class);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ManageEnrollment(int classId)
        {

            var @class = await _context.Classes.Include(x => x.Course)
            .Include(x => x.Lecturer)
            .Include(x => x.Enrollments)
            .ThenInclude(x => x.Student)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == classId);

            var students = await _context.Students.ToListAsync();

            var model = new ClassEnrollmentVM
            {
                Class = new ClassViewModel
                {
                    Id = @class.Id,
                    CourseName = $"{@class.Course.Code}-{@class.Course.Name}",
                    LectruerName = $"{@class.Lecturer.FirstName} {@class.Lecturer.LastName}",
                    Time = @class.Time.ToString()
                },
            };

            foreach (var item in students)
            {
                model.Students.Add(
                    new StudentsEnrollmentVM
                    {
                        Id = item.Id,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        IsEnrolled = (@class?.Enrollments?.Any(z =>
                        {
                            return z.StudentId == item.Id && @class.Id == z.ClassId;
                        }
                        )).GetValueOrDefault()
                    }
                );
            }


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnrollStudent(int classId, int studentId, bool shouldEnroll)
        {
            var enrollment = new Enrollment();
            if (shouldEnroll)
            {
                enrollment.StudentId = studentId;
                enrollment.ClassId = classId;
                await _context.Enrollments.AddAsync(enrollment);
                _notyfService.Success("Student Enrolled Successfully");
            }
            else
            {
                enrollment = await _context.Enrollments.FirstOrDefaultAsync(x => x.ClassId == classId && x.StudentId == studentId);

                if (enrollment != null)
                {
                    _context.Enrollments.Remove(enrollment);
                    _notyfService.Warning("Student Unrolled Successfully");
                }

            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageEnrollment), new { classId });
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }

        private void CreateSelectLists()
        {
            var lecturers = _context.Lecturers.Select(x => new
            {
                FullName = $"{x.FirstName} {x.LastName}",
                x.Id
            });
            var courses = _context.Courses.Select(x => new
            {
                CourseInfo = $"{x.Code}-{x.Name}-({x.Credits}) Credits",
                x.Id,
            });
            ViewData["LecturerId"] = new SelectList(lecturers, "Id", "FullName");
            ViewData["CourseId"] = new SelectList(courses, "Id", "CourseInfo");
        }
    }
}
