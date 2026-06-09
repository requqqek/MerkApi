using System.Security.Cryptography;
using System.Text;

namespace MerkApi.Services
{
    /// <summary>
    /// Обратимое шифрование персональных данных (email, телефон) — AES-256.
    /// Требование 152-ФЗ: ПДн хранятся в защищённом (зашифрованном) виде.
    /// </summary>
    public class CryptoService
    {
        private readonly byte[] _key;

        public CryptoService(IConfiguration configuration)
        {
            var secret = configuration["Encryption:Key"]
                ?? "merk_default_encryption_key_change_me!";
            _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret)); // ровно 32 байта
        }

        public string? Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return Convert.ToBase64String(result);
        }

        public string? Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            try
            {
                var fullBytes = Convert.FromBase64String(cipherText);
                using var aes = Aes.Create();
                aes.Key = _key;

                var iv = new byte[16];
                Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                var cipherBytes = fullBytes[iv.Length..];
                var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return cipherText; // защита от старых/незашифрованных данных
            }
        }
    }
}