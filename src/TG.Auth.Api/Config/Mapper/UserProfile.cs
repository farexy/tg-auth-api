using AutoMapper;
using TG.Auth.Api.Entities;
using TG.Core.ServiceBus.Messages;

namespace TG.Auth.Api.Config.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, NewUserAuthorizationMessage>();
            CreateMap<User, UserCancellationMessage>()
                .ForMember(dest => dest.Type, opt => opt.Ignore());
        }
    }
}