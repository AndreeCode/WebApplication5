using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication5.Models;

namespace WebApplication5.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Service>()
                .HasOne(s => s.Owner)
                .WithMany(u => u.Services)
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ServiceRequest>()
                .HasOne(r => r.Service)
                .WithMany()
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
