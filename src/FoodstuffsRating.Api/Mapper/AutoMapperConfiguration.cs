using AutoMapper;
using FoodstuffsRating.Api.Dto;
using FoodstuffsRating.Data.Models;

namespace FoodstuffsRating.Api.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            this.CreateMap<RegisterUserRequest, User>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.PasswordHash, opt => opt.Ignore())
                .ForMember(x => x.IsEmailConfirmed, opt => opt.Ignore())
                .ForMember(x => x.LastLoginDateUtc, opt => opt.Ignore())
                .ForMember(x => x.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(x => x.LastUpdatedAtUtc, opt => opt.Ignore())
                .ForMember(x => x.RefreshTokens, opt => opt.Ignore());

            this.CreateMap<User, UserProfile>();
        }
    }
}
