using System;
using System.Threading.Tasks;

namespace OGame.Services.Interfaces
{
    public interface IDateTimeService
    {
        Task<DateTime> Now();
    }
}