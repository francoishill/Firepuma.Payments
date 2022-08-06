using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.Exceptions;
using Firepuma.Payments.FunctionApp.PayFast.Config;
using Firepuma.Payments.FunctionApp.PayFast.TableModels;
using Firepuma.Payments.FunctionApp.PayFast.TableProviders;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.PayFast.Queries;

public static class GetPayFastOnceOffPayment
{
    public class Query : IRequest<Result>
    {
        public ClientApplicationId ApplicationId { get; set; }
        public string ApplicationSecret { get; set; }
        public PaymentId PaymentId { get; set; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public PayFastOnceOffPayment OnceOffPayment { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            PayFastOnceOffPayment onceOffPayment,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;

            OnceOffPayment = onceOffPayment;

            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success(PayFastOnceOffPayment onceOffPayment)
        {
            return new Result(true, onceOffPayment, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, null, reason, errors);
        }

        public enum FailureReason
        {
            OnceOffPaymentDoesNotExist,
            ApplicationSecretInvalid,
        }
    }


    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly PayFastClientAppConfigProvider _appConfigProvider;
        private readonly PayFastOnceOffPaymentsTableProvider _payFastOnceOffPaymentsTableProvider;

        public Handler(
            ILogger<Handler> logger,
            PayFastClientAppConfigProvider appConfigProvider,
            PayFastOnceOffPaymentsTableProvider payFastOnceOffPaymentsTableProvider)
        {
            _logger = logger;
            _appConfigProvider = appConfigProvider;
            _payFastOnceOffPaymentsTableProvider = payFastOnceOffPaymentsTableProvider;
        }

        public async Task<Result> Handle(Query query, CancellationToken cancellationToken)
        {
            var applicationId = query.ApplicationId;
            var paymentId = query.PaymentId;

            try
            {
                await _appConfigProvider.ValidateApplicationSecretAsync(
                    applicationId,
                    query.ApplicationSecret,
                    cancellationToken);
            }
            catch (ApplicationSecretInvalidException)
            {
                return Result.Failed(Result.FailureReason.ApplicationSecretInvalid, "Application secret is invalid");
            }

            var onceOffPayment = await LoadOnceOffPayment(applicationId, paymentId, cancellationToken);
            if (onceOffPayment == null)
            {
                _logger.LogCritical("Unable to load onceOffPayment for applicationId: {AppId} and paymentId: {PaymentId}, it was null", applicationId, paymentId);
                return Result.Failed(Result.FailureReason.OnceOffPaymentDoesNotExist, $"PayFast OnceOff payment does not exist, applicationId: {applicationId}, paymentId: {paymentId}");
            }

            return Result.Success(onceOffPayment);
        }

        private async Task<PayFastOnceOffPayment> LoadOnceOffPayment(
            ClientApplicationId applicationId,
            PaymentId paymentId,
            CancellationToken cancellationToken)
        {
            var retrieveOperation = TableOperation.Retrieve<PayFastOnceOffPayment>(applicationId.Value, paymentId.Value);
            var loadResult = await _payFastOnceOffPaymentsTableProvider.Table.ExecuteAsync(retrieveOperation, cancellationToken);

            if (loadResult.Result == null)
            {
                _logger.LogError("loadResult.Result was null for applicationId: {AppId} and paymentId: {PaymentId}", applicationId, paymentId);
                return null;
            }

            return loadResult.Result as PayFastOnceOffPayment;
        }
    }
}