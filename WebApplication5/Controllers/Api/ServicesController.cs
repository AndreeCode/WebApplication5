using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Services.Include(s => s.Owner).Where(s => s.IsPublished).ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _db.Services.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Service model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.Services.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Service model)
        {
            if (id != model.Id) return BadRequest();
            var existing = await _db.Services.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Price = model.Price;
            existing.Currency = model.Currency;
            existing.Category = model.Category;
            existing.Location = model.Location;
            existing.IsPublished = model.IsPublished;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _db.Services.FindAsync(id);
            if (existing == null) return NotFound();
            _db.Services.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("suggest")]
        public async Task<IActionResult> Suggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new string[0]);
            var trimmed = q.Trim();
            var items = await _db.Services.Where(s => EF.Functions.Like(s.Title, $"%{trimmed}%") || EF.Functions.Like(s.Keywords, $"%{trimmed}%")).Select(s => s.Title).Distinct().Take(10).ToListAsync();
            return Ok(items);
        }
    }
}
