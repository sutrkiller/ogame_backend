using AutoMapper;
using OGame.Repositories.Entities;
using OGame.Services.Models;

namespace OGame.Services.MapperProfiler
{
    public class RunnerProfiler : Profile
    {
        public RunnerProfiler()
        {
            CreateMap<Runner, RunnerEntity>();

            CreateMap<RunnerEntity, Runner>();
        }
    }
}
