using System.Text.Json;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Abstractions;
using Firepuma.Payments.Domain.Plumbing.IntegrationEvents.Services;
using MongoDB.Bson;
using Xunit.Abstractions;

namespace Firepuma.Payments.Tests.Domain.Plumbing.IntegrationEvents.Services;

public class IntegrationEventsMappingCacheTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public IntegrationEventsMappingCacheTests(
        ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [MemberData(nameof(IntegrationEventInstancesMemberData))]
    public void All_IntegrationEvent_Payload_types_should_have_event_type_mapping(Type eventPayloadType)
    {
        // Arrange
        var mappingCache = new IntegrationEventsMappingCache();
        var eventPayload = (BaseIntegrationEventProducingCommandResponse)Activator.CreateInstance(eventPayloadType)!;

        // Act
        _testOutputHelper.WriteLine($"EventPayloadType = {eventPayloadType}");
        var successfullyGotEventType = mappingCache.TryGetIntegrationEventType(eventPayload, out var eventType);

        // Assert
        Assert.True(successfullyGotEventType);
        Assert.NotNull(eventType);
        Assert.NotEmpty(eventType);
    }

    [Theory]
    [MemberData(nameof(IntegrationEventTypeStringsWithPayloadInstancesMemberData))]
    public void All_IntegrationEvent_types_should_have_deserialize_mapping(string eventType, BaseIntegrationEventProducingCommandResponse eventProducingCommandResponse)
    {
        // the fact that IntegrationEventTypeStrings is derived from calling TryGetIntegrationEventType means that
        // it implicitly also tests that each type having an EventType mapping can also be deserialized

        // Arrange
        var mappingCache = new IntegrationEventsMappingCache();
        var envelope = new IntegrationEventEnvelope
        {
            EventId = ObjectId.GenerateNewId().ToString(),
            EventType = eventType,
            EventPayload = JsonSerializer.Serialize((dynamic)eventProducingCommandResponse),
        };

        // Act
        _testOutputHelper.WriteLine($"EventType = {eventType}, EventPayload = {envelope.EventPayload}");
        var successfullyGotEventType = mappingCache.TryDeserializeIntegrationEvent(envelope, out var deserializedEvent);

        // Assert
        Assert.True(successfullyGotEventType);
        Assert.NotNull(deserializedEvent);
    }

    [Theory]
    [MemberData(nameof(IntegrationEventTypeStringsOnlyMemberData))]
    public void All_IntegrationEvent_types_should_return_true_for_IsIntegrationEvent(string eventType)
    {
        // Arrange
        var mappingCache = new IntegrationEventsMappingCache();

        // Act
        _testOutputHelper.WriteLine($"EventType = {eventType}");
        var isIntegrationEvent = mappingCache.IsIntegrationEventForFirepumaPayments(eventType);

        // Assert
        Assert.True(isIntegrationEvent);
    }

    [Fact]
    public void IntegrationEvent_types_should_be_unique()
    {
        // Arrange

        // Act
        _testOutputHelper.WriteLine($"Event types = {string.Join(", ", IntegrationEventTypeStringsOnly)}");
        var duplicates = IntegrationEventTypeStringsOnly
            .GroupBy(eventType => eventType)
            .Where(group => group.Count() > 1)
            .ToList();

        // Assert
        foreach (var duplicate in duplicates)
        {
            _testOutputHelper.WriteLine($"Duplicate event type found: {duplicate.Key}");
        }

        Assert.Empty(duplicates);
    }

    public static IEnumerable<object[]> IntegrationEventInstancesMemberData =>
        GetClassesInheritingBaseClass<BaseIntegrationEventProducingCommandResponse>().Select(evt => new object[] { evt });

    private static IEnumerable<string> IntegrationEventTypeStringsOnly =>
        GetClassesInheritingBaseClass<BaseIntegrationEventProducingCommandResponse>()
            .Select(eventPayloadType =>
            {
                var eventPayload = (BaseIntegrationEventProducingCommandResponse)Activator.CreateInstance(eventPayloadType)!;
                if (!new IntegrationEventsMappingCache().TryGetIntegrationEventType(eventPayload, out var eventType))
                {
                    // return null to exclude those without a valid mapping
                    return null;
                }

                return eventType;
            })
            .Where(eventType => eventType != null)
            .Select(eventType => eventType!);

    public static IEnumerable<object[]> IntegrationEventTypeStringsOnlyMemberData =>
        IntegrationEventTypeStringsOnly.Select(eventType => new object[] { eventType });

    public static IEnumerable<object[]> IntegrationEventTypeStringsWithPayloadInstancesMemberData =>
        GetClassesInheritingBaseClass<BaseIntegrationEventProducingCommandResponse>()
            .Select(eventPayloadType =>
            {
                var eventPayload = (BaseIntegrationEventProducingCommandResponse)Activator.CreateInstance(eventPayloadType)!;
                if (!new IntegrationEventsMappingCache().TryGetIntegrationEventType(eventPayload, out var eventType))
                {
                    // return null to exclude those without a valid mapping
                    return null;
                }

                return new object[] { eventType, eventPayload };
            })
            .Where(obs => obs != null)
            .Select(obs => obs!);

    private static IEnumerable<Type> GetClassesInheritingBaseClass<T>() where T : class
    {
        var allTypesEnumerable = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

        var instances = allTypesEnumerable
            .Where(myType => myType is { IsClass: true, IsAbstract: false } && myType.IsSubclassOf(typeof(T)));

        return instances;
    }
}