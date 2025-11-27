using System.ComponentModel.DataAnnotations;

namespace WebApplication5.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string RequesterId { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string RequesterPhone { get; set; } = string.Empty;
        public string RequesterEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Service? Service { get; set; }
    }
}
