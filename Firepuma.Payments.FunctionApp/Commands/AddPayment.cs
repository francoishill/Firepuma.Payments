using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling.TableModels.Attributes;
using Firepuma.Payments.FunctionApp.PaymentGatewayAbstractions;
using Firepuma.Payments.FunctionApp.TableProviders;
using MediatR;
using Microsoft.Azure.Cosmos.Table;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.Commands;

public static class AddPayment
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public PaymentGatewayTypeId GatewayTypeId { get; init; }

        public ClientApplicationId ApplicationId { get; init; }

        [IgnoreCommandAudit]
        public string ApplicationSecret { get; init; }

        public PaymentId PaymentId { get; init; }
        public object RequestDto { get; set; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public Uri RedirectUrl { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            Uri redirectUrl,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;

            RedirectUrl = redirectUrl;

            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success(Uri redirectUrl)
        {
            return new Result(true, redirectUrl, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, null, reason, errors);
        }

        public enum FailureReason
        {
            UnknownGatewayTypeId,
            ApplicationSecretInvalid,
            PaymentAlreadyExists,
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly ApplicationConfigsTableProvider _applicationConfigsTableProvider;
        private readonly PaymentsTableProvider _paymentsTableProvider;

        public Handler(
            IEnumerable<IPaymentGateway> gateways,
            ApplicationConfigsTableProvider applicationConfigsTableProvider,
            PaymentsTableProvider paymentsTableProvider)
        {
            _gateways = gateways;
            _applicationConfigsTableProvider = applicationConfigsTableProvider;
            _paymentsTableProvider = paymentsTableProvider;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var applicationId = command.ApplicationId;
            var paymentId = command.PaymentId;

            var gateway = _gateways.GetFromTypeIdOrNull(command.GatewayTypeId);

            if (gateway == null)
            {
                return Result.Failed(Result.FailureReason.UnknownGatewayTypeId, $"The payment gateway type '{command.GatewayTypeId}' is not supported");
            }

            var applicationConfig = await gateway.GetApplicationConfigAsync(
                _applicationConfigsTableProvider,
                applicationId,
                cancellationToken);

            if (applicationConfig.ApplicationSecret != command.ApplicationSecret)
            {
                return Result.Failed(Result.FailureReason.ApplicationSecretInvalid, $"The application secret is invalid");
            }

            var paymentEntity = await gateway.CreatePaymentTableEntity(
                applicationConfig,
                applicationId,
                paymentId,
                command.RequestDto,
                cancellationToken);

            try
            {
                await _paymentsTableProvider.Table.ExecuteAsync(TableOperation.Insert(paymentEntity), cancellationToken);
            }
            catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                return Result.Failed(Result.FailureReason.PaymentAlreadyExists, $"The payment (id '{paymentId}' and application id '{applicationId}') is already added and cannot be added again");
            }

            var redirectUrl = await gateway.CreateRedirectUri(
                applicationConfig,
                applicationId,
                paymentId,
                command.RequestDto,
                cancellationToken);

            return Result.Success(redirectUrl);
        }
    }
}