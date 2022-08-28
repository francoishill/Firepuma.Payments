namespace Firepuma.Payments.Implementations.CommandHandling.TableModels.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class IgnoreCommandAuditAttribute : Attribute
{
}