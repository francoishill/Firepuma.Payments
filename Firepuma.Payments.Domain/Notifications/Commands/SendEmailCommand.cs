using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities.Attributes;
using Firepuma.Dtos.Email.BusMessages;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using MediatR;

// ReSharper disable UnusedType.Global

namespace Firepuma.Payments.Domain.Notifications.Commands;

public static class SendEmailCommand
{
    public class Payload : BaseCommand<Result>
    {
        public required string Subject { get; init; }

        public required string FromEmail { get; init; }

        public required string ToEmail { get; init; }

        [IgnoreCommandExecution]
        public required string TextBody { get; init; }
    }

    public class Result
    {
    }

    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly IIntegrationEventEnvelopeFactory _envelopeFactory;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;

        public Handler(
            IIntegrationEventEnvelopeFactory envelopeFactory,
            IIntegrationEventPublisher integrationEventPublisher)
        {
            _envelopeFactory = envelopeFactory;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            var sendEmailRequest = new SendEmailRequest
            {
                Subject = payload.Subject,
                FromEmail = payload.FromEmail,
                ToEmail = payload.ToEmail,
                TextBody = payload.TextBody,
            };

            var integrationEventEnvelope = _envelopeFactory.CreateEnvelopeFromObject(sendEmailRequest);

            await _integrationEventPublisher.SendAsync(integrationEventEnvelope, cancellationToken);

            return new Result();
        }
    }
}