using System.Reflection;
using AutoMapper;
using Firepuma.Payments.Worker.Admin.Controllers;

namespace Firepuma.Payments.Tests.Worker;

public class AutoMapperConfigurationTests
{
    [Fact]
    public void WhenProfilesAreConfigured_ItShouldNotThrowException()
    {
        // Arrange
        var config = new MapperConfiguration(configuration =>
        {
            //Uncomment this if we ever add mapping of Enums
            // configuration.EnableEnumMappingValidation();

            configuration.AddMaps(typeof(ManualHealthCheckingController).GetTypeInfo().Assembly);
        });

        // Assert
        config.AssertConfigurationIsValid();
    }
}