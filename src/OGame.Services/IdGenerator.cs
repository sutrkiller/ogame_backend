using System;
using OGame.Services.Interfaces;

namespace OGame.Services
{
    public class IdGenerator : IIdGenerator
    {
        public Guid GenerateId()
            => Guid.NewGuid();
    }
}