using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGame.Repositories.Contexts;
using OGame.Repositories.Entities;
using OGame.Repositories.Interfaces;

namespace OGame.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly ScheduledTasksContext _dbContext;
        public EmailRepository(ScheduledTasksContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<EmailEntity> GetAsync(Guid id)
         => await _dbContext.Emails.FindAsync(id);

        public async Task<IQueryable<EmailEntity>> GetAllAsync()
        {
            return await Task.FromResult(_dbContext.Emails.AsNoTracking());
        }

        public async Task<EmailEntity> AddAsync(EmailEntity email)
        {
            var entry = await _dbContext.Emails.AddAsync(email);
            return entry.Entity;
        }

        public async Task<EmailEntity> UpdateAsync(EmailEntity email)
        {
            var entry = _dbContext.Emails.Update(email);
            return await Task.FromResult(entry.Entity);
        }

        public async Task RemoveAsync(Guid id)
        {
            var email = await GetAsync(id);

            if (email != null)
            {
                _dbContext.Emails.Remove(email);
            }
        }
    }
}
