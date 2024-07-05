using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AuthApi.Helpers;

public static class PasswordHasher {
    private const int SaltSize = 16; // 128-bit salt
    private const int HashSize = 32; // 256-bit hash
    private const int Iterations = 10000; // Number of PBKDF2 iterations

    public static string HashPassword(string password) {
        // Generate a salt
        var salt = new byte[SaltSize];
        using (var randomNumberGenerator = RandomNumberGenerator.Create()) {
            randomNumberGenerator.GetBytes(salt);
        }

        // Derive a 256-bit subkey (use HMACSHA256 with 10,000 iterations)
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Combine salt and hash
        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // Convert to base64
        var hashedPassword = Convert.ToBase64String(hashBytes);

        // Return the combined salt+hash
        return hashedPassword;
    }

    public static bool VerifyPassword(string password, string hashedPassword) {
        // Get the hash bytes
        var hashBytes = Convert.FromBase64String(hashedPassword);

        // Get the salt
        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Derive the subkey using the stored salt
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Compare the results
        for (var i = 0; i < HashSize; i++) {
            if (hashBytes[i + SaltSize] != hash[i]) {
                return false;
            }
        }

        return true;
    }
}