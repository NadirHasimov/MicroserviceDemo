using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProduction)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProduction);
        }

        private static void SeedData(AppDbContext context, bool isProduction)
        {
            if (isProduction)
            {
                System.Console.WriteLine("--->Attempting to apply migrations ...");
                try
                {
                    context.Database.Migrate();
                }
                catch (System.Exception exc)
                {
                    System.Console.WriteLine($"---> Could not run migrations: {exc.Message}");
                }
            }

            if (!context.Platforms.Any())
            {
                System.Console.WriteLine("--->Seedingdata");
                context.Platforms.AddRange(
                    new Platform()
                    {
                        Name = "Dot Net",
                        Publisher = "Microsoft",
                        Cost = "Free"
                    },
                     new Platform()
                     {
                         Name = "SQL Server Express",
                         Publisher = "Microsoft",
                         Cost = "Free"
                     },
                     new Platform()
                     {
                         Name = "Kbernetes",
                         Publisher = "Cloud Native Computing Foundation",
                         Cost = "Free"
                     }
                );
                context.SaveChanges();
            }
            else
            {
                System.Console.WriteLine("--->We already have data");
            }
        }
    }
}