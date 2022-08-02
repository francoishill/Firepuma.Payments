using System;

namespace Firepuma.Payments.FunctionAppManager.Infrastructure.Config;

public class PaymentsServiceOptions
{
    public Uri FunctionsUrl { get; set; }
    public string FunctionsKey { get; set; }
}