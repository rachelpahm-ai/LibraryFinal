using System;
using System.Security.Cryptography;

namespace Library.Helpers
{
    public static class PasswordHelper
    {
        private const int Iterations = 600000;

        public static string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            byte[] salt = new byte[16];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            using (var derive = new Rfc2898DeriveBytes(
                password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = derive.GetBytes(32);

                // Salt, số vòng và hash được lưu chung một chuỗi.
                return "PBKDF2-SHA256$" + Iterations + "$"
                    + Convert.ToBase64String(salt) + "$"
                    + Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string stored)
        {
            if (password == null || string.IsNullOrEmpty(stored))
                return false;

            try
            {
                string[] parts = stored.Split('$');
                int iterations;

                if (parts.Length != 4 ||
                    parts[0] != "PBKDF2-SHA256" ||
                    !int.TryParse(parts[1], out iterations) ||
                    iterations < 1 || iterations > 1000000)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);

                if (salt.Length != 16 || expected.Length != 32)
                    return false;

                using (var derive = new Rfc2898DeriveBytes(
                    password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    byte[] actual = derive.GetBytes(32);
                    int difference = 0;

                    for (int i = 0; i < actual.Length; i++)
                        difference |= actual[i] ^ expected[i];

                    return difference == 0;
                }
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}