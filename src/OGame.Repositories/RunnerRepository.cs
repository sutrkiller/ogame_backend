using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGame.Repositories.Contexts;
using OGame.Repositories.Entities;
using OGame.Repositories.Interfaces;

namespace OGame.Repositories
{
    public class RunnerRepository : IRunnerRepository
    {
        private readonly ApplicationContext _context;

        public RunnerRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<RunnerEntity> GetById(Guid id)
        {
            return await _context.Runners.FindAsync(id);
        }

        public async Task<RunnerEntity> GetByUserId(Guid userId)
        {
            return await _context.Runners.SingleOrDefaultAsync(x => x.UserId == userId);
        }
    }
}
