using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using RestaurantApp.Application.Interfaces.Services;

namespace RestaurantApp.Infrastructure.Services;

public class AesEncryptionService: IEncryptionService
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
            throw new ArgumentException("Key must be 32 bytes and IV must be 16 bytes.");
        }
    }

    public string Encrypt(string plainText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.IV = _iv;
        aesAlg.Mode = CipherMode.CBC;

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);

        using var aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.IV = _iv;
        aesAlg.Mode = CipherMode.CBC;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}