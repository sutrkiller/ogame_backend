using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OGame.Auth.Contexts;
using OGame.Auth.Models;
using OGame.Configuration;
using OGame.Configuration.Settings;

namespace OGame.ScheduledJobs
{
    class Program
    {
        private static readonly IServiceProvider ServiceProvider;

        static Program()
        {
            var configuration = new ConfigurationBuilder()
                .AddSharedConfig()
                .Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddIdentity<ApplicationUser, ApplicationUserRole>()
                .AddEntityFrameworkStores<SecurityContext>()
                .AddDefaultTokenProviders();

            serviceCollection.AddDbContext<SecurityContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SecurityConnection"));
            });

            serviceCollection.ConfigureSettings(configuration);

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
            var accountSettings = ServiceProvider.GetService<IOptions<AccountSettings>>().Value;

            var deleteBefore = DateTime.UtcNow - TimeSpan.FromDays(accountSettings.ExpirationDays);

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
