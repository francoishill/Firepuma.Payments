using AutoMapper;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;

#pragma warning disable CS8618

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Sample.PaymentsClientApp.Simple.Controllers.Responses;

[AutoMap(typeof(PreparePaymentResponse))]
public class PreparePayfastOnceOffPaymentResponse
{
    public Uri RedirectUrl { get; set; }
    public string PaymentId { get; set; }
}