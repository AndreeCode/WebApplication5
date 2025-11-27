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

                // If users exist, assume seeded
                if (await userManager.Users.AnyAsync())
                {
                    logger.LogInformation("Database already seeded with users.");
                    return;
                }

                // Create a single main user who will both request and offer services
                var mainUser = new ApplicationUser
                {
                    UserName = "user@local.test",
                    Email = "user@local.test",
                    FullName = "Usuario Principal",
                    EmailConfirmed = true,
                    PhoneNumber = "+51910000003",
                    PhoneNumberPublic = "+51910000003"
                };

                var result = await userManager.CreateAsync(mainUser, "P@ssw0rd1");
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to create initial user: {Errors}", string.Join(';', result.Errors.Select(e => e.Description)));
                    throw new Exception("Failed to create seed user");
                }

                // Create services: some owned by the main user (they offer), some with no owner (third-party offers)
                var servicesList = new List<Service>
                {
                    new Service { Title = "Desarrollo Web ASP.NET Core", Description = "Desarrollo de aplicaciones web con ASP.NET Core y EF Core.", Price = 1200, Currency = "PEN", Category = "Desarrollo", OwnerId = mainUser.Id, IsPublished = true, Location = "Lima", Keywords = "asp.net,web,desarrollo" },
                    new Service { Title = "Diseño de Logo Moderno", Description = "Diseño profesional de logotipos y branding para startups.", Price = 250, Currency = "PEN", Category = "Diseño", OwnerId = mainUser.Id, IsPublished = true, Location = "Lima", Keywords = "diseño,logo,branding" },
                    new Service { Title = "Redacción de Contenido SEO", Description = "Creación de artículos optimizados para buscadores.", Price = 150, Currency = "PEN", Category = "Contenido", OwnerId = mainUser.Id, IsPublished = true, Location = "Remoto", Keywords = "redacción,contenido,seo" },

                    // Third-party (no owner) offerings
                    new Service { Title = "Soporte Técnico Remoto", Description = "Soporte y mantenimiento de sistemas, atención remota y on-site.", Price = 120, Currency = "PEN", Category = "Soporte", OwnerId = null, IsPublished = true, Location = "Lima", Keywords = "soporte,it,remoto" },
                    new Service { Title = "Fotografía de Producto", Description = "Sesiones de fotografía para e-commerce y catálogo.", Price = 300, Currency = "PEN", Category = "Fotografía", OwnerId = null, IsPublished = true, Location = "Lima", Keywords = "fotografía,producto" },
                    new Service { Title = "Gestión de Redes Sociales", Description = "Planificación y gestión de contenidos en redes sociales.", Price = 350, Currency = "PEN", Category = "Marketing", OwnerId = null, IsPublished = true, Location = "Remoto", Keywords = "social,marketing" }
                };

                context.Services.AddRange(servicesList);
                await context.SaveChangesAsync();

                // Add a sample request from mainUser to a third-party service
                var targetService = await context.Services.FirstOrDefaultAsync(s => s.OwnerId == null);
                if (targetService != null)
                {
                    var req = new ServiceRequest
                    {
                        ServiceId = targetService.Id,
                        RequesterId = mainUser.Id,
                        RequesterName = mainUser.FullName,
                        RequesterPhone = mainUser.PhoneNumber ?? string.Empty,
                        RequesterEmail = mainUser.Email ?? string.Empty,
                        Message = "Hola, estoy interesado en tu servicio. ¿Cuál es tu disponibilidad?",
                        CreatedAt = DateTime.UtcNow
                    };
                    context.ServiceRequests.Add(req);
                    await context.SaveChangesAsync();
                }

                logger.LogInformation("Seeded 1 user and {Services} services.", servicesList.Count);
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
