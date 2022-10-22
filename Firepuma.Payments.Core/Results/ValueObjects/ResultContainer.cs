namespace Firepuma.Payments.Core.Results.ValueObjects;

public class ResultContainer<TSuccess, TFailureReasons>
    where TSuccess : class
    where TFailureReasons : struct
{
    public bool IsSuccessful { get; set; }

    public TSuccess? Result { get; set; }

    public TFailureReasons? FailedReason { get; set; }
    public string[]? FailedErrors { get; set; }

    private ResultContainer(
        bool isSuccessful,
        TSuccess? result,
        TFailureReasons? failedReason,
        string[]? failedErrors)
    {
        IsSuccessful = isSuccessful;

        Result = result;

        FailedReason = failedReason;
        FailedErrors = failedErrors;
    }

    public static ResultContainer<TSuccess, TFailureReasons> Success(TSuccess successfulValue)
    {
        return new ResultContainer<TSuccess, TFailureReasons>(true, successfulValue, null, null);
    }

    public static ResultContainer<TSuccess, TFailureReasons> Failed(TFailureReasons reason, params string[] errors)
    {
        return new ResultContainer<TSuccess, TFailureReasons>(false, null, reason, errors);
    }
}