using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.PaymentAppConfiguration.ValueObjects;
using Firepuma.Payments.Core.Payments.Entities;
using Firepuma.Payments.Core.Payments.Repositories;
using Firepuma.Payments.Core.Payments.ValueObjects;
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
            PaymentDoesNotExist,
        }
    }


    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IPaymentRepository _paymentRepository;

        public Handler(
            ILogger<Handler> logger,
            IPaymentRepository paymentRepository)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
        }

        public async Task<Result> Handle(Query query, CancellationToken cancellationToken)
        {
            var applicationId = query.ApplicationId;
            var paymentId = query.PaymentId;

            var paymentEntity = await _paymentRepository.GetItemOrDefaultAsync(applicationId, paymentId, cancellationToken);

            if (paymentEntity == null)
            {
                _logger.LogCritical(
                    "Unable to load payment for applicationId: {AppId} and paymentId: {PaymentId}, it was null",
                    applicationId, paymentId);

                return Result.Failed(
                    Result.FailureReason.PaymentDoesNotExist,
                    $"Unable to load payment for applicationId: {applicationId} and paymentId: {paymentId}, it was null");
            }

            return Result.Success(paymentEntity);
        }
    }
}