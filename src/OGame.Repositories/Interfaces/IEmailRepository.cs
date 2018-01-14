using System;
using System.Linq;
using System.Threading.Tasks;
using OGame.Repositories.Entities;

namespace OGame.Repositories.Interfaces
{
    public interface IEmailRepository
    {
        Task<EmailEntity> GetAsync(Guid id);
        Task<IQueryable<EmailEntity>> GetAllAsync();

        Task<EmailEntity> AddAsync(EmailEntity email);

        Task<EmailEntity> UpdateAsync(EmailEntity email);

        Task RemoveAsync(Guid id);
    }
}