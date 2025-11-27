using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication5.Models;
using WebApplication5.Data;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const int HomeItems = 6;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _db.Services.Include(s => s.Owner).Where(s => s.IsPublished).OrderByDescending(s => s.CreatedAt).Take(HomeItems).ToListAsync();
            return View(items);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
