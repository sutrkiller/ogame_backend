using System;
using System.Threading.Tasks;
using OGame.Services.Interfaces;

namespace OGame.Services
{
    public class IdProvider : IIdProvider
    {
        public async Task<Guid> NewId()
            => await Task.FromResult(Guid.NewGuid());
    }
}