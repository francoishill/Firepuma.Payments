namespace Firepuma.Payments.Infrastructure.Helpers;

public static class EnvironmentVariableHelpers
{
    public static string GetRequiredEnvironmentVariable(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"Environment variable '{key}' is empty but required");
        }

        return value;
    }
}