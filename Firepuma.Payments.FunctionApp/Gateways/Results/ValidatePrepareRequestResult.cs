using Firepuma.Payments.Core.ClientDtos.ClientRequests.ExtraValues;

namespace Firepuma.Payments.FunctionApp.Gateways.Results;

public class ValidatePrepareRequestResult
{
    public IPreparePaymentExtraValues ExtraValues { get; set; }
}