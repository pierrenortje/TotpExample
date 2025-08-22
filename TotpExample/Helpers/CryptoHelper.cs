using System.Security.Cryptography;
using System.Text;

namespace TotpExample.Helpers
{
    public static class CryptoHelper
    {
        // Encrypts plaintext using AES with a password
        internal static string Encrypt(string plainText, string password)
        {
            // Generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // Derive key and IV from password + salt
            using var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            using var aes = Aes.Create();
            aes.Key = key.GetBytes(32); // 256-bit key
            aes.IV = key.GetBytes(16);  // 128-bit IV

            using var ms = new MemoryStream();
            ms.Write(salt, 0, salt.Length); // prepend salt

            using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cryptoStream, Encoding.UTF8))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        // Decrypts ciphertext using AES with a password
        internal static string Decrypt(string cipherText, string password)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            // Extract salt
            byte[] salt = new byte[16];
            Array.Copy(cipherBytes, 0, salt, 0, salt.Length);

            // Derive key and IV again
            using var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            using var aes = Aes.Create();
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var ms = new MemoryStream(cipherBytes, salt.Length, cipherBytes.Length - salt.Length);
            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cryptoStream, Encoding.UTF8);

            return sr.ReadToEnd();
        }
    }
}
