using System;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Entities;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Repositories;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.Specifications;
using Firepuma.Payments.Core.Infrastructure.ServiceMonitoring.ValueObjects;
using Firepuma.Payments.FunctionAppManager.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.FunctionAppManager.Queries;

public static class CheckDeadMessageCountsQuery
{
    public class Query : IRequest<Result>
    {
        public DateTimeOffset NowDateTimeOffset { get; init; }
    }

    public class Result
    {
        public bool IsDue { get; init; }
        public ServiceAlertState? LastAlertState { get; init; } //TODO: required

        public bool CountIsDifferent { get; init; }
        public int PreviousDeadMessageCount { get; init; }
        public int CurrentDeadMessageCount { get; init; }
    }

    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IServiceAlertStateRepository _serviceAlertStateRepository;
        private readonly IDeadLetteredMessageRepository _deadLetteredMessageRepository;

        public Handler(
            ILogger<Handler> logger,
            IServiceAlertStateRepository serviceAlertStateRepository,
            IDeadLetteredMessageRepository deadLetteredMessageRepository)
        {
            _logger = logger;
            _serviceAlertStateRepository = serviceAlertStateRepository;
            _deadLetteredMessageRepository = deadLetteredMessageRepository;
        }

        public async Task<Result> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            var lastAlertState = await _serviceAlertStateRepository.GetItemOrDefaultAsync(ServiceAlertType.NewDeadLetteredMessages.ToString(), cancellationToken);

            if (lastAlertState != null && lastAlertState.NextCheckTime > query.NowDateTimeOffset)
            {
                return new Result
                {
                    IsDue = false,
                    LastAlertState = lastAlertState,
                };
            }

            int previousDeadMessageCount;

            if (lastAlertState == null)
            {
                previousDeadMessageCount = 0;
            }
            else if (!lastAlertState.TryCastAlertContextToType<NewDeadLetteredMessagesExtraValues>(out var alertContext, out var castError))
            {
                previousDeadMessageCount = 0;
                _logger.LogError("Unable to cast lastAlertState context to NewDeadLetteredMessagesExtraValues, error: {Error}", castError);
            }
            else
            {
                previousDeadMessageCount = alertContext.TotalDeadLetteredMessages;
            }

            var currentDeadMessageCount = await _deadLetteredMessageRepository.GetItemsCountAsync(new AllDeadLetteredMessagesSpecification(), cancellationToken);
            var countIsDifferent = currentDeadMessageCount != previousDeadMessageCount;

            return new Result
            {
                IsDue = true,
                LastAlertState = lastAlertState,

                CountIsDifferent = countIsDifferent,
                PreviousDeadMessageCount = previousDeadMessageCount,
                CurrentDeadMessageCount = currentDeadMessageCount,
            };
        }
    }
}