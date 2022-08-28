using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.Implementations.Payments.TableModels;
using Firepuma.Payments.Implementations.Repositories.EntityRepositories;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.Queries;

public static class GetPaymentDetails
{
    public class Query : IRequest<Result>
    {
        public PaymentGatewayTypeId GatewayTypeId { get; init; }

        public ClientApplicationId ApplicationId { get; set; }

        public PaymentId PaymentId { get; set; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public PaymentEntity PaymentEntity { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            PaymentEntity paymentEntity,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;

            PaymentEntity = paymentEntity;

            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success(PaymentEntity paymentEntity)
        {
            return new Result(true, paymentEntity, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, null, reason, errors);
        }

        public enum FailureReason
        {
            UnknownGatewayTypeId,
            PaymentDoesNotExist,
        }
    }


    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly IPaymentRepository _paymentRepository;

        public Handler(
            ILogger<Handler> logger,
            IEnumerable<IPaymentGateway> gateways,
            IPaymentRepository paymentRepository)
        {
            _logger = logger;
            _gateways = gateways;
            _paymentRepository = paymentRepository;
        }

        public async Task<Result> Handle(Query query, CancellationToken cancellationToken)
        {
            var gatewayTypeId = query.GatewayTypeId;
            var applicationId = query.ApplicationId;
            var paymentId = query.PaymentId;

            var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

            if (gateway == null)
            {
                return Result.Failed(Result.FailureReason.UnknownGatewayTypeId, $"The payment gateway type '{gatewayTypeId}' is not supported");
            }

            //TODO: Is there a better way? The Gateway should decide which specific entity type to load but this Query should decide which Azure Table and handle the loading.
            var paymentEntity = await _paymentRepository.GetItemOrDefaultAsync(applicationId, paymentId, cancellationToken);

            if (paymentEntity == null)
            {
                _logger.LogCritical(
                    "Unable to load payment for gatewayTypeId: {GatewayTypeId}, applicationId: {AppId} and paymentId: {PaymentId}, it was null",
                    gatewayTypeId, applicationId, paymentId);

                return Result.Failed(
                    Result.FailureReason.PaymentDoesNotExist,
                    $"Unable to load payment for gatewayTypeId: {gatewayTypeId}, applicationId: {applicationId} and paymentId: {paymentId}, it was null");
            }

            return Result.Success(paymentEntity);
        }
    }
}