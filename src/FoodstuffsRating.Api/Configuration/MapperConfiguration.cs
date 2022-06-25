using AutoMapper;
using FoodstuffsRating.Data.Models;
using FoodstuffsRating.Dto;
using FoodstuffsRating.Models.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace FoodstuffsRating.Api.Configuration
{
    public static class MapperConfiguration
    {
        public static void Configure(IServiceCollection services)
        {
            var mapperConfig = new AutoMapper.MapperConfiguration(p => 
                p.AddProfile(new MapperProfile()));

            mapperConfig.AssertConfigurationIsValid();
            var mapper = mapperConfig.CreateMapper();

            services.AddSingleton(mapper);
        }
    }

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            this.CreateMap<UserRegistrationRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDateUtc, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalLogins, opt => opt.Ignore());

            this.CreateMap<User, UserProfile>();

            this.CreateMap<UserRegistrationFromExternalRequest, ExternalProviderRegistrationModel>()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LoginProvider, opt => opt.Ignore());

            this.CreateMap<UserProfileUpdateRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDateUtc, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedAtUtc, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalLogins, opt => opt.Ignore());
        }
    }
}
