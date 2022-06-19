using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Firepuma.PaymentsService.FunctionAppManager.Constants;
using Firepuma.PaymentsService.FunctionAppManager.Services.Results;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.PaymentsService.FunctionAppManager.Services;

public class FunctionsHostManager : IFunctionsHostManager
{
    private readonly ILogger<FunctionsHostManager> _logger;
    private readonly HttpClient _httpClient;

    public FunctionsHostManager(
        ILogger<FunctionsHostManager> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(HttpClientConstants.PAYMENTS_SERVICE_FUNCTIONS_HTTP_CLIENT_NAME);
    }

    public async Task<CreateFunctionsHostSecretKeyResult> CreateHostSecretKeyIfNotExists(
        string keyName,
        CancellationToken cancellationToken)
    {
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