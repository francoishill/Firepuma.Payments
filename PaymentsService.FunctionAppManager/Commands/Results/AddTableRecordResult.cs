using Firepuma.PaymentsService.Implementations.Config;

namespace Firepuma.PaymentsService.FunctionAppManager.Commands.Results;

public class AddTableRecordResult
{
    public string TableName { get; set; }
    public bool IsNew { get; set; }
    public ClientAppConfig TableRow { get; set; }
}