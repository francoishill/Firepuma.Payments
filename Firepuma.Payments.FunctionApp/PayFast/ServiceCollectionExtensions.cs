﻿using Firepuma.PaymentsService.FunctionApp.Infrastructure.TableStorage;
using Firepuma.PaymentsService.FunctionApp.PayFast.Config;
using Firepuma.PaymentsService.FunctionApp.PayFast.TableProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.PaymentsService.FunctionApp.PayFast;

public static class ServiceCollectionExtensions
{
    public static void AddPayFastFeature(
        this IServiceCollection services,
        string validateAndStoreItnBaseUrl)
    {
        services.Configure<PayFastOptions>(opt =>
        {
            opt.ValidateAndStoreItnBaseUrl = validateAndStoreItnBaseUrl;
        });

        services.AddTableProvider("PayFastApplicationConfigs", table => new PayFastApplicationConfigsTableProvider(table));
        services.AddTableProvider("PayFastOnceOffPayments", table => new PayFastOnceOffPaymentsTableProvider(table));
        services.AddTableProvider("PayFastItnTraces", table => new PayFastItnTracesTableProvider(table));

        services.AddScoped<PayFastClientAppConfigProvider>();
    }
}