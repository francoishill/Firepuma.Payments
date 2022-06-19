using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionAppManager.Commands.Results;
using Firepuma.PaymentsService.FunctionAppManager.Infrastructure.Constants;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Global

namespace Firepuma.PaymentsService.FunctionAppManager.Commands;

public class CreateFunctionHostSecretKey : IRequest<object>
{
    public string FunctionHostKeyName { get; set; }

    public CreateFunctionHostSecretKey(string functionHostKeyName)
    {
        FunctionHostKeyName = functionHostKeyName;
    }


    public class Handler : IRequestHandler<CreateFunctionHostSecretKey, object>
    {
        private readonly ILogger<Handler> _logger;
        private readonly HttpClient _httpClient;

        public Handler(
            ILogger<Handler> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient(HttpClientConstants.PAYMENTS_SERVICE_FUNCTIONS_HTTP_CLIENT_NAME);
        }

        public async Task<object> Handle(CreateFunctionHostSecretKey command, CancellationToken cancellationToken)
        {
            var keyName = command.FunctionHostKeyName;

            var keyResponse = await _httpClient.GetAsync($"/admin/host/keys/{keyName}", cancellationToken);

            var createResult = new CreateFunctionsHostSecretKeyResult
            {
                KeyName = keyName,
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

                createResult.IsNew = false;
                createResult.KeyValue = key.Value;
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

                createResult.IsNew = true;
                createResult.KeyValue = key.Value;
            }


            return createResult;
        }

        private class KeyResponse
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}