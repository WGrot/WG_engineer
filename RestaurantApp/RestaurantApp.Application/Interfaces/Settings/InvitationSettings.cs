namespace RestaurantApp.Application.Interfaces.Settings;

public class InvitationSettings
{
    public const string SectionName = "InvitationSettings";
    
    public int ExpirationHours { get; set; } = 48;
    public int TokenLength { get; set; } = 32;
}