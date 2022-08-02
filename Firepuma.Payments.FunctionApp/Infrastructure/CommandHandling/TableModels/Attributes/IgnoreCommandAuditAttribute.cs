using System;

namespace Firepuma.PaymentsService.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class IgnoreCommandAuditAttribute : Attribute
{
}