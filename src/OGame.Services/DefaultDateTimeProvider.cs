using System;
using System.Threading.Tasks;
using OGame.Services.Interfaces;

namespace OGame.Services
{
    public class DefaultDateTimeProvider : IDateTimeProvider
    {
        public async Task<DateTime> Now()
            => await Task.FromResult(DateTime.UtcNow);
    }
}
