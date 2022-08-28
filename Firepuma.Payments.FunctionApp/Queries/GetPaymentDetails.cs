using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.Implementations.CommandHandling.TableModels.Attributes;
using Firepuma.Payments.Implementations.Config;
using Firepuma.Payments.Implementations.TableStorage;
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

        [IgnoreCommandAudit]
        public BasePaymentApplicationConfig ApplicationConfig { get; init; }

        public PaymentId PaymentId { get; set; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public IPaymentTableEntity PaymentTableEntity { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            IPaymentTableEntity paymentTableEntity,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;

            PaymentTableEntity = paymentTableEntity;

            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success(IPaymentTableEntity paymentTableEntity)
        {
            return new Result(true, paymentTableEntity, null, null);
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
        private readonly ITableService<IPaymentTableEntity> _paymentsTableService;

        public Handler(
            ILogger<Handler> logger,
            IEnumerable<IPaymentGateway> gateways,
            ITableService<IPaymentTableEntity> paymentsTableService)
        {
            _logger = logger;
            _gateways = gateways;
            _paymentsTableService = paymentsTableService;
        }

        public async Task<Result> Handle(Query query, CancellationToken cancellationToken)
        {
            var gatewayTypeId = query.GatewayTypeId;
            var applicationId = query.ApplicationId;
            var applicationConfig = query.ApplicationConfig;
            var paymentId = query.PaymentId;

            var gateway = _gateways.GetFromTypeIdOrNull(gatewayTypeId);

            if (gateway == null)
            {
                return Result.Failed(Result.FailureReason.UnknownGatewayTypeId, $"The payment gateway type '{gatewayTypeId}' is not supported");
            }

            //TODO: Is there a better way? The Gateway should decide which specific entity type to load but this Query should decide which Azure Table and handle the loading.
            try
            {
                var paymentEntity = await gateway.GetPaymentDetailsAsync(
                    _paymentsTableService,
                    applicationConfig,
                    applicationId.Value,
                    paymentId.Value,
                    cancellationToken);

                return Result.Success(paymentEntity);
            }
            catch (RequestFailedException requestFailedException) when (requestFailedException.Status == (int)HttpStatusCode.NotFound)
            {
                _logger.LogCritical(
                    "Unable to load payment for gatewayTypeId: {GatewayTypeId}, applicationId: {AppId} and paymentId: {PaymentId}, it was null",
                    gatewayTypeId, applicationId, paymentId);

                return Result.Failed(
                    Result.FailureReason.PaymentDoesNotExist,
                    $"Unable to load payment for gatewayTypeId: {gatewayTypeId}, applicationId: {applicationId} and paymentId: {paymentId}, it was null");
            }
        }
    }
}