using System;
using System.Linq;
using System.Threading;
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
using OGame.Configuration.Contracts.Settings;
using OGame.Repositories.Interfaces;
using OGame.Services.Interfaces;

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
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            serviceCollection.ConfigureSettings(configuration);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        static async Task Main()
        {
            var userManager = ServiceProvider.GetService<UserManager<ApplicationUser>>();
            var accountLogger = ServiceProvider.GetService<ILogger<AccountWorker>>();
            var emailLogger = ServiceProvider.GetService<ILogger<EmailWorker>>();
            var accountSettings = ServiceProvider.GetService<IOptions<AccountSettings>>();
            var emailSettings = ServiceProvider.GetService<IOptions<EmailSettings>>();
            var dateTimeProvider = ServiceProvider.GetService<IDateTimeProvider>();
            var emailRepository = ServiceProvider.GetService<IEmailRepository>();

            var workers = new ITaskWorker[]
            {
                new EmailWorker(emailRepository, dateTimeProvider, emailLogger, emailSettings),
                new AccountWorker(userManager, accountLogger, accountSettings, dateTimeProvider)
            };
            var tasks = workers.Select(x => x.Run(CancellationToken.None));

            await Task.WhenAll(tasks);
        }
    }
}
