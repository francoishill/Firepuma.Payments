using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.Payments.Core.Infrastructure.CommandsAndQueries;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Config;
using Firepuma.Payments.FunctionAppManager.Infrastructure.Constants;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.Payments.FunctionAppManager.Commands;

public static class CreateFunctionHostSecretKey
{
    public class Command : BaseCommand, IRequest<Result>
    {
        public string FunctionHostKeyName { get; set; }

        public Command(string functionHostKeyName)
        {
            FunctionHostKeyName = functionHostKeyName;
        }
    }

    public class Result
    {
        public string KeyName { get; set; }
        public bool IsNew { get; set; }
        public string KeyValue { get; set; }
        public string FunctionsBaseUrl { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IOptions<PaymentsServiceOptions> _paymentsServiceOptions;
        private readonly HttpClient _httpClient;

        public Handler(
            ILogger<Handler> logger,
            IOptions<PaymentsServiceOptions> paymentsServiceOptions,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _paymentsServiceOptions = paymentsServiceOptions;
            _httpClient = httpClientFactory.CreateClient(HttpClientConstants.PAYMENTS_SERVICE_FUNCTIONS_HTTP_CLIENT_NAME);
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var keyName = command.FunctionHostKeyName;

            var keyResponse = await _httpClient.GetAsync($"/admin/host/keys/{keyName}", cancellationToken);

            var result = new Result
            {
                KeyName = keyName,
                FunctionsBaseUrl = _paymentsServiceOptions.Value.FunctionsUrl.AbsoluteUri,
            };

            if (keyResponse.IsSuccessStatusCode)
            {
                var keyBody = await keyResponse.Content.ReadAsStringAsync(cancellationToken);
                var key = JsonConvert.DeserializeObject<KeyResponse>(keyBody);

                if (key == null)
                {
                    _logger.LogError("Unable to parse function host key response as KeyResponse, body was: {Body}", keyBody);
                    throw new Exception("Unable to parse function host key response as KeyResponse");
                }

                result.IsNew = false;
                result.KeyValue = key.Value;
            }
            else
            {
                var generatedKeyResponse = await _httpClient.PostAsync($"/admin/host/keys/{keyName}", null, cancellationToken);
                generatedKeyResponse.EnsureSuccessStatusCode();

                var keyBody = await generatedKeyResponse.Content.ReadAsStringAsync(cancellationToken);
                var key = JsonConvert.DeserializeObject<KeyResponse>(keyBody);

                if (key == null)
                {
                    _logger.LogError("Unable to parse (newly generated) function host key response as KeyResponse, body was: {Body}", keyBody);
                    throw new Exception("Unable to parse (newly generated) function host key response as KeyResponse");
                }

                result.IsNew = true;
                result.KeyValue = key.Value;
            }


            return result;
        }

        private class KeyResponse
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}