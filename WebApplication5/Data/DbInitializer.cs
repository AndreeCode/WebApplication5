using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication5.Models;

namespace WebApplication5.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var context = provider.GetRequiredService<ApplicationDbContext>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbInitializer");

            try
            {
                await context.Database.MigrateAsync();

                // Use UserManager to check for existing users to ensure Identity tables are present
                var anyUsers = await userManager.Users.AnyAsync();
                if (anyUsers)
                {
                    logger.LogInformation("Database already seeded with users.");
                    return; // already seeded
                }

                var demo = new ApplicationUser { UserName = "demo@local.test", Email = "demo@local.test", FullName = "Usuario Demo", EmailConfirmed = true };
                var prov = new ApplicationUser { UserName = "provider@local.test", Email = "provider@local.test", FullName = "Proveedor Demo", EmailConfirmed = true };

                await userManager.CreateAsync(demo, "P@ssw0rd1");
                await userManager.CreateAsync(prov, "P@ssw0rd1");

                var servicesList = new List<Service>
                {
                    new Service { Title = "Desarrollo Web ASP.NET Core", Description = "Desarrollo de aplicaciones web con ASP.NET Core y EF Core.", Price = 1200, Currency = "PEN", Category = "Desarrollo", OwnerId = prov.Id, IsPublished = true, Location = "Lima", Keywords = "asp.net,web,desarrollo" },
                    new Service { Title = "Diseño de Logo", Description = "Diseño profesional de logotipos y branding.", Price = 200, Currency = "PEN", Category = "Diseño", OwnerId = demo.Id, IsPublished = true, Location = "Lima", Keywords = "diseño,logo,branding" },
                    new Service { Title = "SEO y Marketing", Description = "Optimización SEO y campañas de marketing digital.", Price = 500, Currency = "PEN", Category = "Marketing", OwnerId = prov.Id, IsPublished = true, Location = "Perú", Keywords = "seo,marketing" },
                    new Service { Title = "Soporte Técnico", Description = "Soporte y mantenimiento de sistemas.", Price = 100, Currency = "PEN", Category = "Soporte", OwnerId = demo.Id, IsPublished = true, Location = "Lima", Keywords = "soporte,it" },
                    new Service { Title = "Fotografía de Producto", Description = "Sesiones de fotografía para e-commerce.", Price = 300, Currency = "PEN", Category = "Fotografía", OwnerId = prov.Id, IsPublished = true, Location = "Lima", Keywords = "fotografía,producto" },
                    new Service { Title = "Redacción de Contenido", Description = "Creación de artículos y contenidos SEO.", Price = 150, Currency = "PEN", Category = "Contenido", OwnerId = demo.Id, IsPublished = true, Location = "Remoto", Keywords = "redacción,contenido,seo" }
                };

                context.Services.AddRange(servicesList);
                await context.SaveChangesAsync();

                logger.LogInformation("Database has been seeded.");
            }
            catch (Exception ex)
            {
                var logger2 = loggerFactory.CreateLogger("DbInitializer");
                logger2.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
