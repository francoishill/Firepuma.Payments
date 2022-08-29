using AutoMapper;
using Firepuma.Payments.Core.ClientDtos.ClientResponses;
using Firepuma.Payments.Core.Payments.ValueObjects;

#pragma warning disable CS8618
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Sample.PaymentsClientApp.Simple.Services.Results;

[AutoMap(typeof(PayFastOnceOffPaymentResponse))]
public class PayfastOnceOffPaymentResult
{
    public PaymentId PaymentId { get; set; }
    public string EmailAddress { get; set; }
    public string NameFirst { get; set; }
    public double ImmediateAmountInRands { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
    public string Status { get; set; }
    public DateTime? StatusChangedOn { get; set; }
}