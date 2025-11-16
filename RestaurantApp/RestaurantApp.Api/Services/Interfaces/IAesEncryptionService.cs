namespace RestaurantApp.Api.Services.Interfaces;

public interface IAesEncryptionService
{
    public string Encrypt(string plainText);
    public string Decrypt(string cipherText);
}