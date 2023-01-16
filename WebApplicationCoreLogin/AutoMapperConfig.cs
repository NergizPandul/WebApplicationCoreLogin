using AutoMapper;
using WebApplicationCoreLogin.Models;
using WebApplicationCoreLogin.Models.ViewModel;

namespace WebApplicationCoreLogin
{
    public class AutoMapperConfig:Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<User, UserViewModel>().ReverseMap();
        }
    }
}
