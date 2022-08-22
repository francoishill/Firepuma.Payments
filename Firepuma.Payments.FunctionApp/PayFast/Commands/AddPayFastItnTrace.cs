using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Abstractions.ValueObjects;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.TableModels;
using Firepuma.Payments.Implementations.TableStorage;
using MediatR;
using Newtonsoft.Json;
using PayFast;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.FunctionApp.PayFast.Commands;

public static class AddPayFastItnTrace
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public ClientApplicationId ApplicationId { get; set; }
        public PayFastNotify PayFastRequest { get; set; }
        public string IncomingRequestUri { get; set; }
    }

    public class Result
    {
        public bool IsSuccessful { get; set; }

        public FailureReason? FailedReason { get; set; }
        public string[] FailedErrors { get; set; }

        private Result(
            bool isSuccessful,
            FailureReason? failedReason,
            string[] failedErrors)
        {
            IsSuccessful = isSuccessful;
            FailedReason = failedReason;
            FailedErrors = failedErrors;
        }

        public static Result Success()
        {
            return new Result(true, null, null);
        }

        public static Result Failed(FailureReason reason, params string[] errors)
        {
            return new Result(false, reason, errors);
        }

        public enum FailureReason
        {
        }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ITableProvider<PaymentNotificationTrace> _payFastItnTracesTableProvider;

        public Handler(
            ITableProvider<PaymentNotificationTrace> payFastItnTracesTableProvider)
        {
            _payFastItnTracesTableProvider = payFastItnTracesTableProvider;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var applicationId = command.ApplicationId;
            var payFastRequest = command.PayFastRequest;

            var payfastNotificationJson = JsonConvert.SerializeObject(payFastRequest, new Newtonsoft.Json.Converters.StringEnumConverter());
            var traceRecord = new PaymentNotificationTrace(
                applicationId,
                new PaymentId(payFastRequest.m_payment_id),
                payFastRequest.pf_payment_id,
                payfastNotificationJson,
                command.IncomingRequestUri);

            await _payFastItnTracesTableProvider.AddEntityAsync(traceRecord, cancellationToken);

            return Result.Success();
        }
    }
}