namespace Firepuma.Payments.Core.Infrastructure.CommandsAndQueries.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class IgnoreCommandAuditAttribute : Attribute
{
}