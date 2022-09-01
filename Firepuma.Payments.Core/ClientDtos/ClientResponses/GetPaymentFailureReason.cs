namespace Firepuma.Payments.Core.ClientDtos.ClientResponses;

public enum GetPaymentFailureReason
{
    BadRequestResponse,
    UnexpectedFailure,
    UnableToDeserializeBody,
}