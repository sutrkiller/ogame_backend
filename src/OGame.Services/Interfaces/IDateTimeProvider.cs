using System;
using System.Threading.Tasks;

namespace OGame.Services.Interfaces
{
    public interface IDateTimeProvider
    {
        Task<DateTime> Now();
    }
}