using System;
using System.Threading.Tasks;
using OGame.Repositories.Entities;

namespace OGame.Repositories.Interfaces
{
    public interface IRunnerRepository
    {
        Task<RunnerEntity> GetById(Guid id);

        Task<RunnerEntity> GetByUserId(Guid userId);
    }
}