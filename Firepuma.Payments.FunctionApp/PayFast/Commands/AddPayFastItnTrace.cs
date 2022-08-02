using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.FunctionApp.Infrastructure.CommandHandling;
using Firepuma.Payments.FunctionApp.PayFast.TableModels;
using Firepuma.Payments.FunctionApp.PayFast.TableProviders;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
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
        public string ApplicationId { get; set; }
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
        private readonly PayFastItnTracesTableProvider _payFastItnTracesTableProvider;

        public Handler(
            PayFastItnTracesTableProvider payFastItnTracesTableProvider)
        {
            _payFastItnTracesTableProvider = payFastItnTracesTableProvider;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var applicationId = command.ApplicationId;
            var payFastRequest = command.PayFastRequest;

            var payfastNotificationJson = JsonConvert.SerializeObject(payFastRequest, new Newtonsoft.Json.Converters.StringEnumConverter());
            var traceRecord = new PayFastItnTrace(
                applicationId,
                payFastRequest.m_payment_id,
                payFastRequest.pf_payment_id,
                payfastNotificationJson,
                command.IncomingRequestUri);

            var insertOperation = TableOperation.Insert(traceRecord);
            await _payFastItnTracesTableProvider.Table.ExecuteAsync(insertOperation, cancellationToken);

            return Result.Success();
        }
    }
}