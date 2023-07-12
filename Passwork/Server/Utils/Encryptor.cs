using System.Security.Cryptography;
using System.Text;

namespace Passwork.Server.Utils;

public static class Encryptor
{
    public static string Encrypt(string password, string key)
    {
        using (Aes aes = Aes.Create())
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            byte[] salt = GenerateSalt(); // Генерируем соль
            byte[] derivedKey = DeriveKey(keyBytes, salt); // Генерируем ключ

            aes.Key = derivedKey;
            aes.IV = new byte[16]; // Инициализируем вектор инициализации

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            byte[] encryptedBytes = encryptor.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);

            byte[] encryptedData = new byte[salt.Length + encryptedBytes.Length];
            Array.Copy(salt, encryptedData, salt.Length);
            Array.Copy(encryptedBytes, 0, encryptedData, salt.Length, encryptedBytes.Length);

            string encryptedPassword = Convert.ToBase64String(encryptedData);
            return encryptedPassword;
        }
    }

    public static string Decrypt(string encryptedPassword, string key)
    {
        using (Aes aes = Aes.Create())
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] encryptedData = Convert.FromBase64String(encryptedPassword);

            byte[] salt = new byte[16];
            byte[] encryptedBytes = new byte[encryptedData.Length - salt.Length];

            Array.Copy(encryptedData, salt, salt.Length);
            Array.Copy(encryptedData, salt.Length, encryptedBytes, 0, encryptedBytes.Length);

            byte[] derivedKey = DeriveKey(keyBytes, salt); // Генерируем ключ

            aes.Key = derivedKey;
            aes.IV = new byte[16]; // Инициализируем вектор инициализации

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            string decryptedPassword = Encoding.UTF8.GetString(decryptedBytes);
            return decryptedPassword;
        }
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[16]; // Размер соли
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt); // Генерируем случайную соль
        }
        return salt;
    }

    private static byte[] DeriveKey(byte[] keyBytes, byte[] salt)
    {
        using (var deriveBytes = new Rfc2898DeriveBytes(keyBytes, salt, 10000)) // Параметр 10000 - количество итераций хеширования
        {
            return deriveBytes.GetBytes(32); // Размер ключа
        }
    }
}
