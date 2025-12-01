using RestaurantApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using RestaurantApp.Shared.Common;


    public class NotificationService
    {
        private readonly List<Notification> _notifications = new();
        
        private CancellationTokenSource _autoHideCts;

        public IEnumerable<Notification> Notifications => _notifications.AsReadOnly();
        public bool IsListVisible { get; private set; }
        public int UnreadCount => _notifications.Count;

        public event Action OnChange;

        public void AddNotification(Notification notification)
        {
            if (notification == null) return;

            _notifications.Add(notification);
            
            IsListVisible = true; 
            NotifyStateChanged(); 
            
            StartAutoHideTimer(); 
        }
        
        public void ClearAll()
        {
            _notifications.Clear();
            HideList(); 
        }
        public void RemoveNotification(Notification notification)
        {
            _notifications.Remove(notification);
            NotifyStateChanged();
        }


        public void ToggleListVisibility()
        {
            IsListVisible = !IsListVisible;
            
            if (IsListVisible)
            {
                _autoHideCts?.Cancel();
            }

            NotifyStateChanged();
        }
        
        public void HideList()
        {
            if (IsListVisible)
            {
                IsListVisible = false;
                _autoHideCts?.Cancel(); 
                NotifyStateChanged();
            }
        }
        
        private async void StartAutoHideTimer()
        {

            _autoHideCts?.Cancel();
            
            _autoHideCts = new CancellationTokenSource();
            var token = _autoHideCts.Token;

            try
            {

                await Task.Delay(6000, token);
                
                if (!token.IsCancellationRequested)
                {
                    IsListVisible = false;
                    NotifyStateChanged();
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
