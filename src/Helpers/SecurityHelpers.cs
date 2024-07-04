using System.Security.Cryptography;

namespace AuthApi.Helpers;

public static class SecurityHelpers {
    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static byte[] GenerateSecureRandom(int length) {
        if (length <= 0)
            throw new ArgumentException("Length must be a positive number.", nameof(length));

        var byteBuffer = new byte[length];
        using var randomGenerator = RandomNumberGenerator.Create();
        randomGenerator.GetBytes(byteBuffer);

        return byteBuffer;
    }

    public static string GenerateSecureRandomBase64(int length) {
        var byteBuffer = GenerateSecureRandom(length);
        return Convert.ToBase64String(byteBuffer);
    }
}