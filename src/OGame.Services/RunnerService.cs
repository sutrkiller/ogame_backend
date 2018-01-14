using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using OGame.Repositories.Interfaces;
using OGame.Services.Interfaces;
using OGame.Services.Models;

namespace OGame.Services
{
    public class RunnerService : IRunnerService
    {
        private readonly IMapper _mapper;
        private readonly IRunnerRepository _runnerRepository;

        public RunnerService(IMapper mapper, IRunnerRepository runnerRepository)
        {
            _mapper = mapper;
            _runnerRepository = runnerRepository;
        }

        public async Task<Runner> GetByUserId(Guid userId)
        {
            var runner = await _runnerRepository.GetByUserId(userId);
            return _mapper.Map<Runner>(runner);
        }

        public Task<Runner> GetById(Guid runnerId)
        {
            throw new NotImplementedException();
        }

        public Task<Runner> Create(Guid userId, string name)
        {
            throw new NotImplementedException();
        }
    }
}
