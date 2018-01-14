using System;
using System.Threading.Tasks;
using OGame.Services.Models;

namespace OGame.Services.Interfaces
{
    public interface IRunnerService
    {
        Task<Runner> GetByUserId(Guid userId);
        Task<Runner> GetById(Guid runnerId);
        Task<Runner> Create(Guid userId, string name);

    }
}
