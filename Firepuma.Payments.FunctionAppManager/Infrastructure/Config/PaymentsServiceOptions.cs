using System;

namespace Firepuma.Payments.FunctionAppManager.Infrastructure.Config;

public class PaymentsServiceOptions
{
    public Uri FunctionsUrl { get; set; } = null!;
    public string FunctionsKey { get; set; } = null!;
}