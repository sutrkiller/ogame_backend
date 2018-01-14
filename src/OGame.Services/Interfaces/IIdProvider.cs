using System;
using System.Threading.Tasks;

namespace OGame.Services.Interfaces
{
    public interface IIdProvider
    {
        Task<Guid> NewId();
    }
}
