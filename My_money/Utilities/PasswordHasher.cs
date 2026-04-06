using System;
using System.Security.Cryptography;

namespace My_money.Utilities
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;      // 128 bit
        private const int KeySize = 32;       // 256 bit
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize);

            return string.Join('.',
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.', 3);
            if (parts.Length != 3)
                return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[2]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                Algorithm,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
