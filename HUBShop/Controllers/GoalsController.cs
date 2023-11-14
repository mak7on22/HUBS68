using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HUBShop.Models;
using HUBShop.Models.Task;
using HUBShop.Enums;
using Microsoft.AspNetCore.Authorization;
using HUBShop.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace HUBShop.Controllers
{
    [Authorize]
    [ResponseCache(CacheProfileName = "Cashing")]
    public class GoalsController : Controller
    {
        private readonly HubContext _context;
        private readonly UserManager<User> _userManager;

        public GoalsController(HubContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        // GET: Goals
        public async Task<IActionResult> Index(string searchWords, DateTime? startDate, DateTime? endDate, Priority? pr, string? st, int pg = 1, TDLSortState sortState = TDLSortState.NameAsc)
        {
            const int pageSize = 10;
            if (pg < 1)
                pg = 1;
            IQueryable<Goal> tdlContext = _context.Goals.Include(g => g.Creator);
            var currentUser = User.Identity.Name; 
            var isAdmin = User.IsInRole("Admin");
            var currentUserTasks = tdlContext
                .Where(g => g.Creator.UserName == currentUser);
            var otherUserTasks = tdlContext
                .Where(g => g.Creator.UserName != currentUser);
            var allTasks = currentUserTasks.Concat(otherUserTasks);
            if (searchWords != null) allTasks = allTasks.Where(x => x.Name == searchWords || x.Description.Contains(searchWords));
            if (startDate != null) allTasks = allTasks.Where(x => x.Created >= startDate.Value);
            if (endDate != null) allTasks = allTasks.Where(x => x.Created <= endDate.Value);
            if (pr != null) allTasks = allTasks.Where(x => x.Priority == pr);
            if (st != null && st != "All") allTasks = allTasks.Where(x => x.Status == st);
            switch (sortState)
            {
                case TDLSortState.PriorityValueAsc: tdlContext = tdlContext.OrderBy(p => p.PriorityValue); break;
                case TDLSortState.StatusValueAsc: tdlContext = tdlContext.OrderBy(p => p.StatusValue); break;
                case TDLSortState.CreatedAsc: tdlContext = tdlContext.OrderBy(p => p.Created); break;
                case TDLSortState.NameDesc: allTasks = allTasks.OrderByDescending(p => p.Name); break;
                case TDLSortState.PriorityValueDesc: allTasks = allTasks.OrderByDescending(p => p.PriorityValue); break;
                case TDLSortState.StatusValueDesc: allTasks = allTasks.OrderByDescending(p => p.StatusValue);  break;
                case TDLSortState.CreatedDesc: allTasks = allTasks.OrderByDescending(p => p.Created); break;
                case TDLSortState.NameAsc: tdlContext = tdlContext.OrderBy(p => p.Name); break;
            }
            var filteredGoals = await allTasks.ToListAsync();

            int recsCount = filteredGoals.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = filteredGoals
                .Skip(recSkip)
                .Take(pageSize)
                .ToList();
            this.ViewBag.Pager = pager;
            ViewBag.NameSort = sortState == TDLSortState.NameAsc ? TDLSortState.NameDesc : TDLSortState.NameAsc;
            ViewBag.PriorityValueSort = sortState == TDLSortState.PriorityValueAsc ? TDLSortState.PriorityValueDesc : TDLSortState.PriorityValueAsc;
            ViewBag.StatusValueSort = sortState == TDLSortState.StatusValueAsc ? TDLSortState.StatusValueDesc : TDLSortState.StatusValueAsc;
            ViewBag.CreatedSort = sortState == TDLSortState.CreatedAsc ? TDLSortState.CreatedDesc : TDLSortState.CreatedAsc;

            return View(data);
        }

        // GET: Goals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Goals == null)
            {
                return NotFound();
            }

            var goal = await _context.Goals
                .Include(g => g.Creator)
                .Include(g => g.Executor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        // GET: Goals/Create
        public IActionResult Create()
        {
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Goals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Priority,PriorityValue,Status,StatusValue,Description,Created,Started,Ended,CreatorId,ExecutorId")] Goal goal)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                goal.CreatorId = user.Id;
                goal.Created = DateTime.UtcNow;

                // Создайте список для хранения ошибок валидации
                var validationErrors = new List<ValidationResult>();

                // Используйте TryValidateObject для выполнения валидации модели
                if (Validator.TryValidateObject(goal, new ValidationContext(goal), validationErrors, validateAllProperties: true))
                {
                    try
                    {
                        _context.Add(goal);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        while (ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                        }
                        ModelState.AddModelError("Status", ex.Message);
                    }
                }
                else
                {

                    foreach (var error in validationErrors)
                    {
                        foreach (var memberName in error.MemberNames)
                        {
                            ModelState.AddModelError(memberName, error.ErrorMessage);
                        }
                    }
                }
            }

            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", goal.CreatorId);
            ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "Id", goal.ExecutorId);
            return View(goal);
        }


        // GET: Goals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Goals == null)
            {
                return NotFound();
            }

            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", goal.CreatorId);
            ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "Id", goal.ExecutorId);
            return View(goal);
        }

        // POST: Goals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PriorityId,Priority,PriorityValue,Status,StatusValue,Description,Created,Started,Ended,CreatorId,ExecutorId")] Goal goal)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingGoal = await _context.Goals.FindAsync(id);
                    if (existingGoal == null)
                    {
                        return NotFound();
                    }

                    if (existingGoal.Status == "В процессе" || existingGoal.Status == "Завершена")
                    {
                        ModelState.AddModelError("Status", "Нельзя редактировать задачу в данном состоянии.");
                        return View(goal);
                    }

                    if (goal.Status != null)
                    {
                        existingGoal.Name = goal.Name;
                        existingGoal.Priority = goal.Priority;
                        existingGoal.Description = goal.Description;
                        existingGoal.CreatorId = goal.CreatorId;


                        _context.Update(existingGoal);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("Status", "Неверное состояние задачи.");
                    }
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("Status", ex.Message);
                }
            }
            #region
            //if (id != goal.Id)
            //{
            //    return NotFound();
            //}

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        _context.Update(goal);
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!GoalExists(goal.Id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}
            #endregion
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", goal.CreatorId);
            ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "Id", goal.ExecutorId);
            return View(goal);
        }

        // GET: Goals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Goals == null)
            {
                return NotFound();
            }

            var goal = await _context.Goals
                .Include(g => g.Creator)
                .Include(g => g.Executor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        // POST: Goals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Goals == null)
            {
                return Problem("Entity set 'HubContext.Goals'  is null.");
            }
            var goal = await _context.Goals.FindAsync(id);
            if (goal != null)
            {
                _context.Goals.Remove(goal);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GoalExists(int id)
        {
          return (_context.Goals?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeGoal(int id)
        {
            var goal = await _context.Goals.FindAsync(id);

            if (goal == null)
                return NotFound();
            // Получите Id текущего аутентифицированного пользователя (здесь предполагается, что у вас есть система аутентификации)
            string currentUserId = User.Identity.Name; // Пример, используйте свой способ получения Id пользователя

            // Преобразуйте currentUserId в int, если это безопасно
            if (int.TryParse(currentUserId, out int userId))
            {
                // Установите ExecutorId на Id текущего пользователя
                goal.ExecutorId = userId;

                // Обновите запись задачи в базе данных
                _context.Update(goal);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Обработайте ситуацию, когда currentUserId не может быть преобразован в int
                // Можно выбрать другое действие или вернуть сообщение об ошибке
                return BadRequest("Неверный формат идентификатора пользователя.");
            }
        }
        public async Task<IActionResult> Start(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (goal.CreatorId == int.Parse(userId))
            {
                ModelState.AddModelError(string.Empty, "Создатель не может выполнять свои же задачи");
                return View(nameof(Index), await _context.Goals.ToListAsync());
            }

            try
            {
                if (goal.Status == "Новая")
                {
                    goal.Started = DateTime.Now;
                    goal.Status = "В процессе";
                    _context.Update(goal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else if (goal.Status == "В процессе")
                {
                    ModelState.AddModelError(string.Empty, "Задача уже находится в процессе выполнения.");
                    return View(nameof(Index), await _context.Goals.ToListAsync());
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Нельзя начать выполнение задачи в данном состоянии.");
                    return View(nameof(Index), await _context.Goals.ToListAsync());
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Произошла ошибка при запуске задачи.");
                return View(nameof(Index), await _context.Goals.ToListAsync());
            }
        }



        public async Task<IActionResult> Complete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var goal = await _context.Goals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            try
            {
                if (goal.Status == "В процессе")
                {
                    goal.Ended = DateTime.Now;
                    goal.Status = "Завершена";
                    _context.Update(goal);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Нельзя завершить задачу в данном состоянии.");
                    return View(nameof(Index), await _context.Goals.ToListAsync());
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Произошла ошибка при завершении задачи.");
                return View(nameof(Index), await _context.Goals.ToListAsync());
            }
        }
        public async Task<IActionResult> Indexs(string sortOrder)
        {
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var goals = from g in _context.Goals
                        select g;

            switch (sortOrder)
            {
                case "name_desc":
                    goals = goals.OrderByDescending(g => g.Creator);
                    break;
                default:
                    goals = goals.OrderBy(g => g.Creator);
                    break;
            }

            return View(await goals.ToListAsync());
        }
    }
}

