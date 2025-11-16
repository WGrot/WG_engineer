using System.Security.Cryptography;
using RestaurantApp.Api.Services.Interfaces;

namespace RestaurantApp.Api.Services;

public class AesEncryptionService: IAesEncryptionService
{

    private readonly byte[] _key; 
    private readonly byte[] _iv;

    public AesEncryptionService(IConfiguration configuration)
    {
        
        var base64Key = configuration["EncryptionSettings:Key"];
        var base64IV = configuration["EncryptionSettings:IV"];

        _key = Convert.FromBase64String(base64Key);
        _iv = Convert.FromBase64String(base64IV);

        if (_key.Length != 32 || _iv.Length != 16)
        {
            throw new ArgumentException("Klucz musi mieć 32 bajty, a IV 16 bajtów.");
        }
    }
    
    public string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = _key;
            aesAlg.IV = _iv;
            aesAlg.Mode = CipherMode.CBC; // Standardowy i bezpieczny tryb

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }

    public string Decrypt(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = _key;
            aesAlg.IV = _iv;
            aesAlg.Mode = CipherMode.CBC;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}