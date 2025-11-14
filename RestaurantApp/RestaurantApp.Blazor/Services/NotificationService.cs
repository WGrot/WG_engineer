using RestaurantApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using RestaurantApp.Shared.Common;


    public class NotificationService
    {
        private readonly List<Notification> _notifications = new();
        
        // --- NOWY ELEMENT ---
        // Token do anulowania timera, jeśli użytkownik kliknie dzwonek
        private CancellationTokenSource _autoHideCts;

        public IEnumerable<Notification> Notifications => _notifications.AsReadOnly();
        public bool IsListVisible { get; private set; }
        public int UnreadCount => _notifications.Count;

        public event Action OnChange;

        // --- ZMODYFIKOWANA METODA ---
        public void AddNotification(Notification notification)
        {
            if (notification == null) return;

            _notifications.Add(notification);
            
            IsListVisible = true; // Automatycznie pokaż listę
            NotifyStateChanged(); // Poinformuj UI (w tym odznakę i listę)
            
            StartAutoHideTimer(); // Uruchom timer do ukrycia
        }

        // ... Metody RemoveNotification i ClearAll pozostają bez zmian ...
        // (Chociaż możesz chcieć, aby ClearAll też ukrywało listę)
        public void ClearAll()
        {
            _notifications.Clear();
            HideList(); // Ukryj listę po wyczyszczeniu
        }
        public void RemoveNotification(Notification notification)
        {
            _notifications.Remove(notification);
            NotifyStateChanged();
        }


        // --- ZMODYFIKOWANA METODA ---
        public void ToggleListVisibility()
        {
            IsListVisible = !IsListVisible;

            // Jeśli użytkownik RĘCZNIE otwiera listę,
            // anulujemy timer auto-ukrywania.
            if (IsListVisible)
            {
                _autoHideCts?.Cancel();
            }

            NotifyStateChanged();
        }

        // --- ZMODYFIKOWANA METODA ---
        public void HideList()
        {
            if (IsListVisible)
            {
                IsListVisible = false;
                _autoHideCts?.Cancel(); // Zawsze anuluj timer przy ukrywaniu
                NotifyStateChanged();
            }
        }

        // --- NOWA PRYWATNA METODA ---
        private async void StartAutoHideTimer()
        {
            // Anuluj poprzedni timer, jeśli jeszcze działał
            _autoHideCts?.Cancel();
            
            // Stwórz nowy token anulowania
            _autoHideCts = new CancellationTokenSource();
            var token = _autoHideCts.Token;

            try
            {
                // Ustaw opóźnienie (np. 4 sekundy)
                await Task.Delay(2000, token);

                // Jeśli przez 4 sekundy nikt nie anulował zadania
                // (np. klikając dzwonek), ukryj listę.
                if (!token.IsCancellationRequested)
                {
                    IsListVisible = false;
                    NotifyStateChanged();
                }
            }
            catch (TaskCanceledException)
            {
                // To jest oczekiwany wyjątek, gdy anulujemy timer.
                // Po prostu go ignorujemy.
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
