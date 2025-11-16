namespace RestaurantApp.Shared.DTOs.Auth.TwoFactor;

public class Enable2FAResponse
{
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public byte[] QrCodeImage { get; set; } = Array.Empty<byte>();
}