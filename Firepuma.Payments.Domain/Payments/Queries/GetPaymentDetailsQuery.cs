﻿using Firepuma.Payments.Domain.Payments.Entities;
using Firepuma.Payments.Domain.Payments.QuerySpecifications;
using Firepuma.Payments.Domain.Payments.Repositories;
using Firepuma.Payments.Domain.Payments.ValueObjects;
using MediatR;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Firepuma.Payments.Domain.Payments.Queries;

public static class GetPaymentDetailsQuery
{
    public class Payload : IRequest<Result>
    {
        public required ClientApplicationId ApplicationId { get; init; }

        public required PaymentId PaymentId { get; init; }
    }

    public class Result
    {
        public required PaymentEntity? PaymentEntity { get; init; }
    }


    public class Handler : IRequestHandler<Payload, Result>
    {
        private readonly IPaymentRepository _paymentRepository;

        public Handler(
            IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Result> Handle(Payload payload, CancellationToken cancellationToken)
        {
            var applicationId = payload.ApplicationId;
            var paymentId = payload.PaymentId;

            var querySpecification = new PaymentByPaymentIdQuerySpecification(applicationId, paymentId);
            var paymentEntity = await _paymentRepository.GetItemOrDefaultAsync(querySpecification, cancellationToken);

            return new Result
            {
                PaymentEntity = paymentEntity,
            };
        }
    }
}