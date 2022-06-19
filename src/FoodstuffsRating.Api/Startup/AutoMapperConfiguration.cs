using AutoMapper;
using FoodstuffsRating.Api.Mapper;

namespace FoodstuffsRating.Api.Startup
{
    public static class AutoMapperConfiguration
    {
        public static void Configure(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(p => 
                p.AddProfile(new MapperProfile()));

            mapperConfig.AssertConfigurationIsValid();
            var mapper = mapperConfig.CreateMapper();

            services.AddSingleton(mapper);
        }
    }
}
