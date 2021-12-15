using AutoMapper;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Models.Response;

namespace TG.Auth.Api.Config.Mapper
{
    public class BanProfile : Profile
    {
        public BanProfile()
        {
            CreateMap<Ban, BanResponse>()
                .ForMember(dest => dest.UserLogin, opt =>
                    opt.MapFrom(src => src.User == null ? null : src.User.Login));
        }
    }
}