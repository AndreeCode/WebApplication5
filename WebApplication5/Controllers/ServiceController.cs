using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication5.Data;
using WebApplication5.Models;
using WebApplication5.ViewModels;

namespace WebApplication5.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ServiceController> _logger;
        private const int PageSize = 6;

        public ServiceController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger<ServiceController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _db.Services.Include(s => s.Owner).Where(s => s.IsPublished).OrderByDescending(s => s.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.CurrentPage = page;
            // search form defaults
            ViewBag.IsSearch = false;
            ViewBag.Query = string.Empty;
            ViewBag.Category = string.Empty;
            ViewBag.MinPrice = null;
            ViewBag.MaxPrice = null;
            ViewBag.Location = string.Empty;
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var service = await _db.Services.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();

            // Determine whether the current user can view the owner's contact (email/phone)
            var currentUserId = _userManager.GetUserId(User);
            var canViewContact = false;
            var hasRequested = false;
            int currentRequestId = 0;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                if (currentUserId == service.OwnerId)
                {
                    canViewContact = true; // owner can see their own contact
                }
                else
                {
                    // check if current user has already made a request for this service
                    var req = await _db.ServiceRequests.FirstOrDefaultAsync(r => r.ServiceId == service.Id && r.RequesterId == currentUserId);
                    if (req != null)
                    {
                        canViewContact = true;
                        hasRequested = true;
                        currentRequestId = req.Id;
                    }
                }
            }

            ViewBag.CanViewContact = canViewContact;
            ViewBag.HasRequested = hasRequested;
            ViewBag.CurrentRequestId = currentRequestId;
            return View(service);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            model.OwnerId = user.Id;
            model.CreatedAt = DateTime.UtcNow;
            _db.Services.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Servicio creado correctamente.";
            return RedirectToAction("Dashboard", "User");
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
        [ValidateAntiForgeryToken]
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
            TempData["Success"] = "Servicio guardado correctamente.";
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (service.OwnerId != user.Id) return Forbid();
            _db.Services.Remove(service);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Servicio eliminado.";
            return RedirectToAction("Dashboard", "User");
        }

        public async Task<IActionResult> Search(string q, string? category, decimal? minPrice, decimal? maxPrice, string? location, int page = 1)
        {
            // detect if any filter provided
            var hasFilter = !string.IsNullOrWhiteSpace(q) || !string.IsNullOrWhiteSpace(category) || minPrice.HasValue || maxPrice.HasValue || !string.IsNullOrWhiteSpace(location);
            ViewBag.IsSearch = true; // indicate we are in search page
            ViewBag.Query = q;
            ViewBag.Category = category ?? string.Empty;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Location = location ?? string.Empty;

            if (!hasFilter)
            {
                // no filters: return empty list and instruction message
                ViewBag.Message = "Introduce criterios de búsqueda para ver resultados.";
                ViewBag.TotalPages = 0;
                ViewBag.CurrentPage = 1;
                return View("Index", new List<Service>());
            }

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestService(int ServiceId, string Message)
        {
            var user = await _userManager.GetUserAsync(User);
            var service = await _db.Services.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == ServiceId);
            if (service == null) return NotFound();
            // Create a ServiceRequest entity with requester's contact info
            var req = new ServiceRequest
            {
                ServiceId = ServiceId,
                RequesterId = user.Id,
                RequesterName = user.FullName,
                RequesterPhone = user.PhoneNumberPublic ?? string.Empty,
                RequesterEmail = user.Email ?? string.Empty,
                Message = Message,
                CreatedAt = DateTime.UtcNow
            };
            _db.Add(req);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Solicitud enviada. Ahora puedes ver los datos de contacto del proveedor.";
            return RedirectToAction("Details", new { id = ServiceId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(int requestId)
        {
            var user = await _userManager.GetUserAsync(User);
            var req = await _db.ServiceRequests.FindAsync(requestId);
            if (req == null) return NotFound();
            if (req.RequesterId != user.Id) return Forbid();
            _db.ServiceRequests.Remove(req);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Solicitud cancelada.";
            return RedirectToAction("Details", new { id = req.ServiceId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var service = await _db.Services.FindAsync(id);
            if (service == null) return NotFound();
            if (service.OwnerId != user.Id) return Forbid();
            service.IsPublished = !service.IsPublished;
            service.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Success"] = service.IsPublished ? "Servicio publicado." : "Servicio despublicado.";
            return RedirectToAction("Dashboard", "User");
        }
    }
}
