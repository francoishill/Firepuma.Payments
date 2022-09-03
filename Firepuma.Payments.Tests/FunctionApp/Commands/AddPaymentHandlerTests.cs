using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.ValueObjects;
using Firepuma.Payments.FunctionApp.Commands;

namespace Firepuma.Payments.Tests.FunctionApp.Commands;

public class AddPaymentHandlerTests
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
        var url = AddPayment.Handler.AddApplicationIdToPaymentNotificationBaseUrl(startingValidationUrl, gatewayTypeId, applicationId);

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
                expectedOutputUrl = "https://my-url.com/My-Gateway/test-app-1",
            },
            new
            {
                gatewayTypeId = new PaymentGatewayTypeId("My-Gateway"),
                applicationId = new ClientApplicationId("test-app-1"),
                startingValidationUrl = "https://my-url.com/random/suburl",
                expectedOutputUrl = "https://my-url.com/random/suburl/My-Gateway/test-app-1",
            },
            new
            {
                gatewayTypeId = new PaymentGatewayTypeId("My-Gateway"),
                applicationId = new ClientApplicationId("test-app-1"),
                startingValidationUrl = "https://my-url.com?code=123",
                expectedOutputUrl = "https://my-url.com/My-Gateway/test-app-1?code=123",
            },
            new
            {
                gatewayTypeId = new PaymentGatewayTypeId("My-Gateway"),
                applicationId = new ClientApplicationId("test-app-1"),
                startingValidationUrl = "https://my-url.com/random/suburl?code=123&param2=value2",
                expectedOutputUrl = "https://my-url.com/random/suburl/My-Gateway/test-app-1?code=123&param2=value2",
            },
        }
        .Select(x => new object[] { x.gatewayTypeId, x.applicationId, x.startingValidationUrl, x.expectedOutputUrl });
}