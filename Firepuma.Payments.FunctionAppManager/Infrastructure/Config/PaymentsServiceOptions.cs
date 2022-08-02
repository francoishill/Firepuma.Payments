using System;

namespace Firepuma.PaymentsService.FunctionAppManager.Infrastructure.Config;

public class PaymentsServiceOptions
{
    public Uri FunctionsUrl { get; set; }
    public string FunctionsKey { get; set; }
}