namespace Firepuma.Payments.Core.Infrastructure.CommandHandling.TableModels.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class IgnoreCommandAuditAttribute : Attribute
{
}