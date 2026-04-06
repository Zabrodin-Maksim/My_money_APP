using System.Security.Cryptography;

namespace My_money.Services
{
    public class PasswordGenerator
    {
        private const string AllowedChars =
        "ABCDEFGHJKLMNPQRSTUVWXYZ" +
        "abcdefghijkmnopqrstuvwxyz" +
        "23456789" +
        "!@#$%";

        public static string Generate(int length = 10)
        {
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = AllowedChars[bytes[i] % AllowedChars.Length];
            }

            return new string(result);
        }
    }
}
}
