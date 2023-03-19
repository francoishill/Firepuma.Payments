using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Google.Cloud.Diagnostics.Common;

namespace Firepuma.Payments.Infrastructure.Plumbing.GoogleLogging.Config;

public class GoogleLoggingOptions
{
    [Required]
    public bool? IsEnabled { get; set; }

    [Required]
    public BufferType? BufferType { get; set; }

    public int? SizedBufferBytes { get; set; }

    public int? TimeBufferWaitTimeMilliseconds { get; set; }

    [Required]
    public RetryType? RetryType { get; set; }

    [Required]
    public ExceptionHandling? RetryExceptionHandling { get; set; }

    public bool Validate([NotNullWhen(false)] out string? validationError)
    {
        if (IsEnabled == null)
        {
            validationError = "IsEnabled is required";
            return false;
        }

        if (IsEnabled == true)
        {
            if (BufferType == null)
            {
                validationError = "BufferType is required";
                return false;
            }

            if (RetryType == null)
            {
                validationError = "RetryType is required";
                return false;
            }
        }

        validationError = null;
        return true;
    }
}