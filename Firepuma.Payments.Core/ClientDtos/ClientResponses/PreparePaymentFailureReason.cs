namespace Firepuma.Payments.Core.ClientDtos.ClientResponses;

public enum PreparePaymentFailureReason
{
    ValidationFailed,
    BadRequestResponse,
    UnexpectedFailure,
    UnableToDeserializeBody,
}