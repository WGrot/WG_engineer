using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using RestaurantApp.Application.Interfaces.Services;

namespace RestaurantApp.Infrastructure.Services;

public class AesEncryptionService : IEncryptionService
{
    private const int KeySize = 32;     
    private const int NonceSize = 12;    
    private const int TagSize = 16;      

    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var base64Key = configuration["EncryptionSettings:Key"];
        _key = Convert.FromBase64String(base64Key);

        if (_key.Length != KeySize)
        {
            throw new ArgumentException("Encryption key must be 32 bytes (AES-256).");
        }
    }

    public string Encrypt(string plainText)
    {
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var result = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        if (fullCipher.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Invalid encrypted payload.");
        }

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var ciphertext = new byte[fullCipher.Length - NonceSize - TagSize];

        Buffer.BlockCopy(fullCipher, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(fullCipher, NonceSize, ciphertext, 0, ciphertext.Length);
        Buffer.BlockCopy(fullCipher, NonceSize + ciphertext.Length, tag, 0, TagSize);

        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }
}