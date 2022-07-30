using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage.Helpers;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableProviders;
using Firepuma.PaymentsService.FunctionApp.PayFast.Validation;
using Firepuma.PaymentsService.Implementations.Config;

namespace Firepuma.PaymentsService.FunctionApp.PayFast.Config;

public class PayFastClientAppConfigProvider
{
    private readonly PayFastApplicationConfigsTableProvider _applicationConfigsTableProvider;

    public PayFastClientAppConfigProvider(
        PayFastApplicationConfigsTableProvider applicationConfigsTableProvider)
    {
        _applicationConfigsTableProvider = applicationConfigsTableProvider;
    }

    public async Task<PayFastClientAppConfig> GetApplicationConfigAndSkipSecretCheckAsync(
        string applicationId,
        CancellationToken cancellationToken)
    {
        var applicationConfig = await AzureTableHelper.GetSingleRecordOrNullAsync<PayFastClientAppConfig>(
            _applicationConfigsTableProvider.Table,
            "PayFast",
            applicationId,
            cancellationToken);

        if (applicationConfig == null)
        {
            throw new Exception($"Config not found for application with id {applicationId}");
        }

        if (!applicationConfig.ValidateClientAppConfig(applicationId, out var validationStatusCode, out var validationErrors))
        {
            throw new Exception($"Client app config is invalid, status code is {validationStatusCode.ToString()}, errors: {validationErrors?.ToArray() ?? new[] { "Validation failed" }}");
        }

        return applicationConfig;
    }

    public async Task<PayFastClientAppConfig> GetApplicationConfigAsync(
        string applicationId,
        string appSecret,
        CancellationToken cancellationToken)
    {
        var applicationConfig = await GetApplicationConfigAndSkipSecretCheckAsync(applicationId, cancellationToken);

        if (applicationConfig.ApplicationSecret != appSecret)
        {
            throw new Exception("ApplicationSecret is invalid");
        }

        return applicationConfig;
    }

    public async Task ValidateApplicationSecretAsync(
        string applicationId,
        string appSecret,
        CancellationToken cancellationToken)
    {
        var applicationConfig = await GetApplicationConfigAndSkipSecretCheckAsync(applicationId, cancellationToken);

        if (applicationConfig.ApplicationSecret != appSecret)
        {
            throw new Exception("ApplicationSecret is invalid");
        }
    }
}