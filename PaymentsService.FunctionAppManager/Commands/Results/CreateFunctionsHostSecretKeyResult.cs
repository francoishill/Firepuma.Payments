﻿namespace Firepuma.PaymentsService.FunctionAppManager.Commands.Results;

public class CreateFunctionsHostSecretKeyResult
{
    public string KeyName { get; set; }
    public bool IsNew { get; set; }
    public string KeyValue { get; set; }
    public string FunctionsBaseUrl { get; set; }
}