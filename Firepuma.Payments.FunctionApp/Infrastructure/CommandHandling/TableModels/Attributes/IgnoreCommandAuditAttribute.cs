using System;

namespace Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class IgnoreCommandAuditAttribute : Attribute
{
}