using Firepuma.Payments.Infrastructure.Plumbing.GoogleLogging.Config;
using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Firepuma.Payments.Infrastructure.Plumbing.GoogleLogging;

public static class GoogleLoggingExtensions
{
    public static void AddCustomGoogleLogging(
        this ILoggingBuilder logging,
        IConfigurationSection googleLoggingConfigSection)
    {
        if (googleLoggingConfigSection == null) throw new ArgumentNullException(nameof(googleLoggingConfigSection));

        var googleLoggingOptions = googleLoggingConfigSection.Get<GoogleLoggingOptions>()!;

        if (googleLoggingOptions == null)
        {
            throw new Exception($"{googleLoggingConfigSection.Path} config section is missing/empty");
        }

        if (!googleLoggingOptions.Validate(out var validationError))
        {
            throw new Exception($"GoogleLogging options invalid: {validationError}");
        }

        if (googleLoggingOptions.IsEnabled == false)
        {
            return;
        }

        var bufferOptions = CreateBufferOptions(googleLoggingOptions);
        var retryOptions = CreateRetryOptions(googleLoggingOptions);

        logging.ClearProviders();
        logging.AddGoogle(new LoggingServiceOptions
        {
            ProjectId = null, // leave null because it is running in Google Cloud when in non-Development mode

            Options = LoggingOptions.Create(
                LogLevel.Trace,
                bufferOptions: bufferOptions,
                retryOptions: retryOptions,
                labels: new Dictionary<string, string>
                {
                    ["x-instance-id"] = Guid.NewGuid().ToString(),
                }
            ),
        });
    }

    private static BufferOptions CreateBufferOptions(GoogleLoggingOptions googleLoggingOptions)
    {
        switch (googleLoggingOptions.BufferType)
        {
            case BufferType.None:
                return BufferOptions.NoBuffer();

            case BufferType.Sized:
                if (googleLoggingOptions.SizedBufferBytes == null)
                {
                    throw new InvalidOperationException($"SizedBufferBytes is required for buffer type {googleLoggingOptions.BufferType.ToString()}");
                }

                return BufferOptions.SizedBuffer(googleLoggingOptions.SizedBufferBytes.Value);

            case BufferType.Timed:
                if (googleLoggingOptions.TimeBufferWaitTimeMilliseconds == null)
                {
                    throw new InvalidOperationException($"TimeBufferWaitTimeMilliseconds is required for buffer type {googleLoggingOptions.BufferType.ToString()}");
                }

                return BufferOptions.TimedBuffer(TimeSpan.FromMilliseconds(googleLoggingOptions.TimeBufferWaitTimeMilliseconds.Value));

            default:
                throw new ArgumentOutOfRangeException(nameof(googleLoggingOptions.BufferType));
        }
    }

    private static RetryOptions CreateRetryOptions(GoogleLoggingOptions googleLoggingOptions)
    {
        switch (googleLoggingOptions.RetryType)
        {
            case RetryType.None:
                if (googleLoggingOptions.RetryExceptionHandling == null)
                {
                    throw new InvalidOperationException($"RetryExceptionHandling is required for retry type {googleLoggingOptions.RetryType.ToString()}");
                }

                return RetryOptions.NoRetry(googleLoggingOptions.RetryExceptionHandling!.Value);

            case RetryType.Retry:
                if (googleLoggingOptions.RetryExceptionHandling == null)
                {
                    throw new InvalidOperationException($"RetryExceptionHandling is required for retry type {googleLoggingOptions.RetryType.ToString()}");
                }

                return RetryOptions.Retry(googleLoggingOptions.RetryExceptionHandling!.Value);

            default:
                throw new ArgumentOutOfRangeException(nameof(googleLoggingOptions.RetryType));
        }
    }
}