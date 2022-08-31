using Firepuma.Payments.FunctionApp.Gateways;

namespace Firepuma.Payments.Tests.FunctionApp.Gateways;

public class PaymentGatewaysTests
{
    [Theory]
    [MemberData(nameof(PaymentGatewayInstancesMemberData))]
    public void PaymentGateway_must_be_non_null(IPaymentGateway gateway)
    {
        Assert.NotNull(gateway);
    }

    [Theory]
    [MemberData(nameof(PaymentGatewayInstancesMemberData))]
    public void PaymentGateway_must_have_a_TypeId(IPaymentGateway gateway)
    {
        Assert.NotNull(gateway.TypeId.Value);
        Assert.NotEmpty(gateway.TypeId.Value);
    }

    [Fact]
    public void Must_have_at_least_one_PaymentGateway_implementations()
    {
        Assert.NotEmpty(PaymentGatewayInstances);
    }

    [Fact(Skip = "Disabled until we have a second payment gateway")]
    public void Must_have_multiple_PaymentGateway_implementations()
    {
        // This test simply asserts that there is more than one gateway implementation available (to ensure we don't accidentally remove all)
        Assert.NotInRange(PaymentGatewayInstances.Count(), 0, 1);
    }

    [Fact]
    public void All_PaymentGateway_implementations_must_have_unique_TypeId_case_insensitive()
    {
        // Arrange
        var duplicateTypeIds = PaymentGatewayInstances
            .GroupBy(g => g.TypeId.Value.ToLower())
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(i => i.TypeId));

        // Assert
        Assert.Empty(duplicateTypeIds);
    }

    [Fact]
    public void All_PaymentGateway_implementations_must_have_unique_DisplayName_case_insensitive()
    {
        // Arrange
        var duplicateDisplayNames = PaymentGatewayInstances
            .GroupBy(g => g.DisplayName.ToLower())
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Select(i => i.DisplayName));

        // Assert
        Assert.Empty(duplicateDisplayNames);
    }

    private static readonly Type _paymentGatewayInterface = typeof(IPaymentGateway);

    private static IEnumerable<IPaymentGateway> PaymentGatewayInstances =>
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => _paymentGatewayInterface.IsAssignableFrom(type) && type.IsClass)
            .Select(gateway => Activator.CreateInstance(gateway) as IPaymentGateway);

    public static IEnumerable<object[]> PaymentGatewayInstancesMemberData => PaymentGatewayInstances.Select(instance => new object[] { instance });
}