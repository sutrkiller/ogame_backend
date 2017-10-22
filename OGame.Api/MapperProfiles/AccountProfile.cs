using AutoMapper;
using OGame.Api.Models.AccountViewModels;
using OGame.Auth.Models;

namespace OGame.Api.MapperProfiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<ApplicationUser, UserViewModel>();
        }
    }
}
