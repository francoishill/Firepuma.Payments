namespace Firepuma.PaymentsService.FunctionAppManager.Services.Results;

public class CreateFunctionsHostSecretKeyResult
{
    public string KeyName { get; set; }
    public bool IsNew { get; set; }
    public string KeyValue { get; set; }
}