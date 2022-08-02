using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.FunctionApp.Infrastructure.Exceptions;
using Firepuma.Payments.FunctionApp.Infrastructure.TableStorage.Helpers;
using Firepuma.Payments.FunctionApp.PayFast.TableProviders;
using Firepuma.Payments.FunctionApp.PayFast.Validation;
using Firepuma.Payments.Implementations.Config;

namespace Firepuma.Payments.FunctionApp.PayFast.Config;

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
            throw new ApplicationSecretInvalidException("ApplicationSecret is invalid");
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
            throw new ApplicationSecretInvalidException("ApplicationSecret is invalid");
        }
    }
}