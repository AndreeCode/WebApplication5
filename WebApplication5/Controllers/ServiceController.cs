using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;
using WebApplication5.Models;
using WebApplication5.ViewModels;

namespace WebApplication5.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PageSize = 6;

        public ServiceController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _db.Services.Include(s => s.Owner).Where(s => s.IsPublished).OrderByDescending(s => s.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.CurrentPage = page;
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var service = await _db.Services.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();
            return View(service);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Service model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            model.OwnerId = user.Id;
            model.CreatedAt = DateTime.UtcNow;
            _db.Services.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (service.OwnerId != user.Id) return Forbid();
            return View(service);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(Service model)
        {
            if (!ModelState.IsValid) return View(model);
            var service = await _db.Services.FindAsync(model.Id);
            if (service == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (service.OwnerId != user.Id) return Forbid();
            service.Title = model.Title;
            service.Description = model.Description;
            service.Price = model.Price;
            service.Currency = model.Currency;
            service.Category = model.Category;
            service.Location = model.Location;
            service.IsPublished = model.IsPublished;
            service.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return RedirectToAction("Dashboard", "User");
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (service.OwnerId != user.Id) return Forbid();
            return View(service);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (service.OwnerId != user.Id) return Forbid();
            _db.Services.Remove(service);
            await _db.SaveChangesAsync();
            return RedirectToAction("Dashboard", "User");
        }

        public async Task<IActionResult> Search(string q, string? category, decimal? minPrice, decimal? maxPrice, string? location, int page = 1)
        {
            var query = _db.Services.Include(s => s.Owner).Where(s => s.IsPublished);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var ql = q.Trim();
                query = query.Where(s => EF.Functions.Like(s.Title, $"%{ql}%") || EF.Functions.Like(s.Keywords, $"%{ql}%"));
            }
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(s => s.Category == category);
            if (minPrice.HasValue) query = query.Where(s => s.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(s => s.Price <= maxPrice.Value);
            if (!string.IsNullOrWhiteSpace(location)) query = query.Where(s => s.Location == location);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(s => s.CreatedAt).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Query = q;
            return View("Index", items);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RequestService(int ServiceId, string Message)
        {
            var user = await _userManager.GetUserAsync(User);
            var service = await _db.Services.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == ServiceId);
            if (service == null) return NotFound();
            // Simple persistence of a request - create a ServiceRequest entity inline (could be a separate model)
            var req = new ServiceRequest { ServiceId = ServiceId, RequesterId = user.Id, Message = Message, CreatedAt = DateTime.UtcNow };
            _db.Add(req);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Solicitud enviada";
            return RedirectToAction("Details", new { id = ServiceId });
        }
    }
}
