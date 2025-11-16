namespace RestaurantApp.Shared.DTOs.Users;

public class ResponseUserLoginDto : ResponseUserDto
{
    public bool TwoFactorEnabled { get; set; } 
}