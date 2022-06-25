using System.Linq;
using System.Reflection;
using FoodstuffsRating.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

namespace FoodstuffsRating.Api.Tests.Configuration
{
    public class DependencyInjectionConfigurationTests
    {
        [Fact]
        public void Configure_BaseFlow_AllInterfacesRegisteredAsServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act
            DependencyInjectionConfiguration.Configure(serviceCollection);

            // Assert
            var assembly = Assembly.Load("FoodstuffsRating.Services");
            var serviceInterfaces = assembly.GetTypes().Where(x => x.IsInterface);
            foreach (var serviceInterface in serviceInterfaces)
            {
                bool interfaceRegistered = serviceCollection.Any(x => x.ServiceType == serviceInterface);
                if (!interfaceRegistered)
                {
                    string message = $"Interface {serviceInterface.Name} not registered in ServiceCollection";
                    throw new XunitException(message);
                }
            }
        }
    }
}
