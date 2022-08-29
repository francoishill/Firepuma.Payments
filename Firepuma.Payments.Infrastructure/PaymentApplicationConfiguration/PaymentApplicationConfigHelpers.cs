using System.Security.Cryptography;

namespace Firepuma.Payments.Infrastructure.Config;

public static class PaymentApplicationConfigHelpers
{
    public static string GenerateRandomSecret()
    {
        var key256 = new byte[32];

        using (var rngCryptoServiceProvider = RandomNumberGenerator.Create())
        {
            rngCryptoServiceProvider.GetBytes(key256);
        }

        return Convert.ToBase64String(key256);
    }
}