namespace RestaurantApp.Blazor.Models;

public class FrontendMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public FrontendMessageType Type { get; set; } = FrontendMessageType.Info;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum FrontendMessageType
{
    Success,
    Error,
    Warning,
    Info
}