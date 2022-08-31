namespace Firepuma.Payments.Core.Infrastructure.CommandHandling.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class IgnoreCommandAuditAttribute : Attribute
{
}