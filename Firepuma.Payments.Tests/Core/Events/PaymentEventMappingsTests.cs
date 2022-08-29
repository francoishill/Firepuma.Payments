using Azure.Messaging.EventGrid;
using Firepuma.Payments.Core.Events;

namespace Firepuma.Payments.Tests.Core.Events;

public class PaymentEventMappingsTests
{
    [Theory]
    [MemberData(nameof(PaymentEventGridMessagesMemberData))]
    public void GetEventTypeName_should_support_all_PaymentEventGridMessage_implementations(IPaymentEventGridMessage paymentEventGridMessageImplementation)
    {
        // Act
        var typeName = PaymentEventMappings.GetEventTypeName(paymentEventGridMessageImplementation);

        // Assert
        Assert.NotNull(typeName);
        Assert.NotEmpty(typeName);
    }

    [Theory]
    [MemberData(nameof(PaymentEventGridMessagesMemberData))]
    public void TryGetPaymentEventData_should_support_all_PaymentEventGridMessage_implementations(IPaymentEventGridMessage paymentEventGridMessageImplementation)
    {
        // Arrange
        // Note: this will fail if it is not added to the EventTypeNameMap (tested by the above unit test All_IPaymentEventGridMessage_implementations_should_be_in_EventTypeNameMap)
        var eventTypeName = PaymentEventMappings.GetEventTypeName(paymentEventGridMessageImplementation);
        var eventGridEvent = new EventGridEvent("sub", eventTypeName, "0.0.1-dev", new Dictionary<string, object>());

        // Act
        var hasGetPaymentEventData = PaymentEventMappings.TryGetPaymentEventData(eventGridEvent, out var eventData);

        // Assert
        Assert.True(hasGetPaymentEventData);
        Assert.NotNull(eventData);
    }

    private static readonly Type _paymentEventGridMessageInterface = typeof(IPaymentEventGridMessage);

    private static IEnumerable<IPaymentEventGridMessage> PaymentEventGridMessageInstances =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => _paymentEventGridMessageInterface.IsAssignableFrom(type) && type.IsClass)
            .Select(gateway => Activator.CreateInstance(gateway) as IPaymentEventGridMessage);

    public static IEnumerable<object[]> PaymentEventGridMessagesMemberData => PaymentEventGridMessageInstances.Select(instance => new object[] { instance });
}