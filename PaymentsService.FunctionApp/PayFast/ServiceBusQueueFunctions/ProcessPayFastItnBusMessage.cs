using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Firepuma.PaymentsService.Abstractions.DTOs.Events;
using Firepuma.PaymentsService.Abstractions.Infrastructure.Queues;
using Firepuma.PaymentsService.Abstractions.ValueObjects;
using Firepuma.PaymentsService.FunctionApp.PayFast.DTOs.Events;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableModels;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PayFast;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.ServiceBusQueueFunctions;

public static class ProcessPayFastItnBusMessage
{
    [FunctionName("ProcessPayFastItnBusMessage")]
    public static async Task RunAsync(
        ILogger log,
        [ServiceBusTrigger("payfast-itn-requests", Connection = "FirepumaPaymentsServiceBus")] ServiceBusReceivedMessage busReceivedMessage,
        [Table("PayFastItnTraces")] IAsyncCollector<PayFastItnTrace> itnTracesCollector,
        [Table("PayFastOnceOffPayments")] CloudTable paymentsTable,
        ServiceBusClient client,
        CancellationToken cancellationToken)
    {
        var serviceBusClient = client; // we have to do this and cannot rename the parameter to serviceBusClient, otherwise the function fails to bind on startup

        var messageJson = busReceivedMessage.Body.ToString();

        log.LogInformation("C# ServiceBus queue trigger function processing message: {QueueMessage}", messageJson);

        var dto = JsonConvert.DeserializeObject<PayFastPaymentItnValidatedEvent>(messageJson);
        if (dto == null)
        {
            log.LogCritical("Dto is null, unable to deserialize messageJson to PayFastPaymentItnValidatedEvent");
            throw new Exception("Dto is null, unable to deserialize messageJson to PayFastPaymentItnValidatedEvent");
        }

        var applicationId = dto.ApplicationId;
        var payFastRequest = dto.PayFastRequest;

        var busMessageSender = serviceBusClient.CreateSender(QueueNameFormatter.GetPaymentUpdatedQueueName(applicationId));

        try
        {
            var payfastNotificationJson = JsonConvert.SerializeObject(payFastRequest);
            var traceRecord = new PayFastItnTrace(
                applicationId,
                payFastRequest.m_payment_id,
                payFastRequest.pf_payment_id,
                payfastNotificationJson,
                dto.IncomingRequestUri);

            await itnTracesCollector.AddAsync(traceRecord, cancellationToken);
            await itnTracesCollector.FlushAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            log.LogError(exception, "Unable to record PayfastItnTrace, exception was: {Message}, stack trace: {Stack}", exception.Message, exception.StackTrace);
        }

        log.LogInformation("Payment status is {Status}", payFastRequest.payment_status);

        var onceOffPayment = await LoadOnceOffPayment(log, paymentsTable, applicationId, payFastRequest.m_payment_id, cancellationToken);
        if (onceOffPayment == null)
        {
            log.LogCritical("Unable to load onceOffPayment for applicationId: {AppId} and paymentId: {PaymentId}, it was null", applicationId, payFastRequest.m_payment_id);
            throw new Exception("Unable to load onceOffPayment");
        }

        if (payFastRequest.payment_status == PayFastStatics.CompletePaymentConfirmation)
        {
            if (string.IsNullOrWhiteSpace(payFastRequest.token))
            {
                log.LogWarning("PayFast ITN for paymentId '{PaymentId}' does not have a payfastToken", payFastRequest.m_payment_id);
            }

            onceOffPayment.SetStatus(PayFastSubscriptionStatus.UpToDate);
            onceOffPayment.PayfastPaymentToken = payFastRequest.token;
        }
        else if (payFastRequest.payment_status == PayFastStatics.CancelledPaymentConfirmation)
        {
            onceOffPayment.SetStatus(PayFastSubscriptionStatus.Cancelled);
        }
        else
        {
            throw new Exception("Unsupported status");
        }

        await SavePaymentAndAddServiceBusMessage(
            log,
            busReceivedMessage.CorrelationId,
            paymentsTable,
            busMessageSender,
            onceOffPayment,
            cancellationToken);
    }

    private static async Task<PayFastOnceOffPayment> LoadOnceOffPayment(
        ILogger log,
        CloudTable paymentsTable,
        string applicationId,
        string paymentId,
        CancellationToken cancellationToken)
    {
        var retrieveOperation = TableOperation.Retrieve<PayFastOnceOffPayment>(applicationId, paymentId);
        var loadResult = await paymentsTable.ExecuteAsync(retrieveOperation, cancellationToken);

        if (loadResult.Result == null)
        {
            log.LogError("loadResult.Result was null for applicationId: {AppId} and paymentId: {PaymentId}", applicationId, paymentId);
            return null;
        }

        return loadResult.Result as PayFastOnceOffPayment;
    }

    private static async Task SavePaymentAndAddServiceBusMessage(
        ILogger log,
        string correlationId,
        CloudTable paymentsTable,
        ServiceBusSender serviceBusSender,
        PayFastOnceOffPayment onceOffPayment,
        CancellationToken cancellationToken)
    {
        var replaceOperation = TableOperation.Replace(onceOffPayment);

        try
        {
            await paymentsTable.ExecuteAsync(replaceOperation, cancellationToken);

            var status = Enum.Parse<PayFastSubscriptionStatus>(onceOffPayment.Status);
            
            var paymentDto = new PayFastPaymentUpdatedEvent(
                onceOffPayment.PaymentId,
                status,
                onceOffPayment.StatusChangedOn,
                correlationId);

            var messageJson = JsonConvert.SerializeObject(paymentDto);

            var busMessage = new ServiceBusMessage(messageJson)
            {
                CorrelationId = correlationId,
                ApplicationProperties =
                {
                    { "applicationId", onceOffPayment.ApplicationId }, // not really used now but add it for easier traceability
                },
            };
            await serviceBusSender.SendMessageAsync(busMessage, cancellationToken);
        }
        catch (StorageException storageException) when (storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
        {
            log.LogCritical(storageException, "Unable to update table due to ETag mismatch, exception message is: {Message}", storageException.Message);
            throw new Exception($"Unable to update table due to ETag mismatch, exception message is: {storageException.Message}");
        }
    }
}