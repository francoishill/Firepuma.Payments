using Firepuma.Payments.Domain.Payments.Commands;
using Firepuma.Payments.Domain.Payments.ValueObjects;

namespace Firepuma.Payments.Tests.Domain.Payments.Commands;

public class AddPaymentCommandHandlerTests
{
    [Theory]
    [MemberData(nameof(AddApplicationIdToPaymentNotificationBaseUrlCases))]
    public void AddApplicationIdToPaymentNotificationBaseUrl_ExpectedBehavior(
        PaymentGatewayTypeId gatewayTypeId,
        ClientApplicationId applicationId,
        string startingValidationUrl,
        string expectedOutputUrl)
    {
        // Act
        var url = AddPaymentCommand.Handler.AddApplicationIdToPaymentNotificationBaseUrl(startingValidationUrl, applicationId, gatewayTypeId);

        // Assert
        Assert.Equal(expectedOutputUrl, url);
    }

    public static IEnumerable<object[]> AddApplicationIdToPaymentNotificationBaseUrlCases = new[]
        {
            new
            {
                gatewayTypeId = new PaymentGatewayTypeId("My-Gateway"),
                applicationId = new ClientApplicationId("test-app-1"),
                startingValidationUrl = "https://my-url.com",
                expectedOutputUrl = "https://my-url.com/test-app-1/My-Gateway",
            },
            new
            {
                gatewayTypeId = new PaymentGatewayTypeId("My-Gateway"),
                applicationId = new ClientApplicationId("test-app-1"),
                startingValidationUrl = "https://my-url.com/random/suburl",
                expectedOutputUrl = "https://my-url.com/random/suburl/test-app-1/My-Gateway",
            },
            new
            {
                gatewayTypeId = new PaymentGatewayTypeId("My-Gateway"),
                applicationId = new ClientApplicationId("test-app-1"),
                startingValidationUrl = "https://my-url.com?code=123",
                expectedOutputUrl = "https://my-url.com/test-app-1/My-Gateway?code=123",
            },
            new
            {
                gatewayTypeId = new PaymentGatewayTypeId("My-Gateway"),
                applicationId = new ClientApplicationId("test-app-1"),
                startingValidationUrl = "https://my-url.com/random/suburl?code=123&param2=value2",
                expectedOutputUrl = "https://my-url.com/random/suburl/test-app-1/My-Gateway?code=123&param2=value2",
            },
        }
        .Select(x => new object[] { x.gatewayTypeId, x.applicationId, x.startingValidationUrl, x.expectedOutputUrl });
}