using System.Security.Cryptography;
using System.Text;

namespace AuthApi.Helpers.Security;

public static class SecurityHelpers {
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

    public static string Sha512(string input) {
        // Create a SHA512 instance
        using var sha512 = SHA512.Create();
        // Convert the input string to a byte array
        var inputBytes = Encoding.UTF8.GetBytes(input);

        // Compute the hash
        var hashBytes = sha512.ComputeHash(inputBytes);

        // Convert the hash bytes to a hexadecimal string
        var sb = new StringBuilder();
        foreach (var b in hashBytes) {
            sb.Append(b.ToString("x2"));
        }

        // Return the hexadecimal string
        return sb.ToString();
    }
}