using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OGame.Auth.Contexts;
using OGame.Auth.Models;

namespace OGame.ScheduledJobs
{
    class Program
    {
        private static readonly IServiceProvider ServiceProvider;

        static Program()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings-private.json")
                .Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddIdentity<ApplicationUser, ApplicationUserRole>()
                .AddEntityFrameworkStores<SecurityContext>()
                .AddDefaultTokenProviders();

            serviceCollection.AddDbContext<SecurityContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SecurityConnection"));
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        static async Task Main()
        {
            await DeleteUncofirmedAccounts();
        }

        private static async Task<int> DeleteUncofirmedAccounts()
        {
            var userManager = ServiceProvider.GetService<UserManager<ApplicationUser>>();
            var logger = ServiceProvider.GetService<ILogger<Program>>();
            var deleteBefore = DateTime.UtcNow - TimeSpan.FromDays(30);

            var usersToDelete = userManager.Users.Where(u => !u.EmailConfirmed && u.JoinDate < deleteBefore);
            await usersToDelete.ForEachAsync(async u =>
            {
                logger.LogInformation($"Deleting account with email '{u.Email}'.");
                await userManager.DeleteAsync(u);
            });
            return usersToDelete.Count();
        }
    }
}
