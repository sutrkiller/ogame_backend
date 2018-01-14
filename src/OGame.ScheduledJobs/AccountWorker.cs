using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OGame.Auth.Models;
using OGame.Configuration.Contracts.Settings;
using OGame.Services.Interfaces;

namespace OGame.ScheduledJobs
{
    public class AccountWorker : ITaskWorker
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountWorker> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly AccountSettings _accountSettings;
        private static DateTime _lastRun;
        private readonly TimeSpan _waitTimeSuccess = TimeSpan.FromMinutes(60);
        private readonly TimeSpan _waitTimeOther = TimeSpan.FromMinutes(10);

        public AccountWorker(UserManager<ApplicationUser> userManager, ILogger<AccountWorker> logger, IOptions<AccountSettings> accountSettings, IDateTimeProvider dateTimeProvider)
        {
            _userManager = userManager;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _accountSettings = accountSettings.Value;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = await _dateTimeProvider.Now();
                if (_lastRun.Date != now.Date)
                {
                    var deleteBefore = now - TimeSpan.FromDays(_accountSettings.ExpirationDays);

                    var usersToDelete = _userManager.Users.Where(u => !u.EmailConfirmed && u.JoinDate < deleteBefore);
                    await usersToDelete.ForEachAsync(async u =>
                    {
                        _logger.LogInformation($"Deleting account with email '{u.Email}'.");
                        await _userManager.DeleteAsync(u);
                    }, cancellationToken);

                    _logger.LogInformation($"Deleted {usersToDelete.Count()} accounts.");
                    _lastRun = now;
                    await Task.Delay(_waitTimeSuccess, cancellationToken);
                }
                else
                {
                    await Task.Delay(_waitTimeOther, cancellationToken);
                }
            }
        }
    }
}
