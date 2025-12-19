using RestaurantApp.Blazor.Models;

namespace RestaurantApp.Blazor.Services;

public class MessageService : IDisposable
{
    private readonly List<FrontendMessage> _messages = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _autoRemoveTokens = new();
    
    private const int AutoRemoveDelayMs = 5000;

    public event Action? OnChange;

    public List<FrontendMessage> GetMessages() => _messages.ToList();

    public void AddMessage(string title, string content, FrontendMessageType type = FrontendMessageType.Info)
    {
        var message = new FrontendMessage
        {
            Title = title,
            Content = content,
            Type = type
        };
        
        AddMessage(message);
    }

    public void AddMessage(FrontendMessage message)
    {
        _messages.Insert(0, message);
        NotifyStateChanged();
        StartAutoRemoveTimer(message);
    }

    public void AddSuccess(string title, string content) 
        => AddMessage(title, content, FrontendMessageType.Success);

    public void AddError(string title, string content) 
        => AddMessage(title, content, FrontendMessageType.Error);

    public void AddWarning(string title, string content) 
        => AddMessage(title, content, FrontendMessageType.Warning);

    public void AddInfo(string title, string content) 
        => AddMessage(title, content, FrontendMessageType.Info);

    public Task RemoveNotificationAsync(FrontendMessage message)
    {
        CancelAutoRemove(message.Id);
        _messages.Remove(message);
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    public void Clear()
    {
        foreach (var cts in _autoRemoveTokens.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _autoRemoveTokens.Clear();
        _messages.Clear();
        NotifyStateChanged();
    }

    private async void StartAutoRemoveTimer(FrontendMessage message)
    {
        var cts = new CancellationTokenSource();
        _autoRemoveTokens[message.Id] = cts;

        try
        {
            await Task.Delay(AutoRemoveDelayMs, cts.Token);

            if (!cts.Token.IsCancellationRequested)
            {
                _messages.Remove(message);
                _autoRemoveTokens.Remove(message.Id);
                cts.Dispose();
                NotifyStateChanged();
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void CancelAutoRemove(Guid messageId)
    {
        if (_autoRemoveTokens.TryGetValue(messageId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _autoRemoveTokens.Remove(messageId);
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        foreach (var cts in _autoRemoveTokens.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _autoRemoveTokens.Clear();
    }
}