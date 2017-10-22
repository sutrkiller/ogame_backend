using System;
using System.Threading.Tasks;
using OGame.Services.Interfaces;

namespace OGame.Services
{
    public class DateTimeService : IDateTimeService
    {
        public async Task<DateTime> Now()
            => await Task.FromResult(DateTime.Now);
    }
}
