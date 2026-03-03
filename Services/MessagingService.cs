using NYC311Dashboard.Components;
using NYC311Dashboard.Services.Contracts;
using System;

namespace NYC311Dashboard.Services
{
    public enum MessageType
    {
        None,
        Info,
        Error
    }

    public class MessagingService : IMessagingService
    {
        public ConfirmDialog confirmDialog { get; set; }
        public event Action? OnMessageChanged;
        public MessageType CurrentType { get; set; } = MessageType.Info;
        public string? Message { get; set; } = Resources.messaging_service_default_message;

        private Func<Task>? _onConfirm;

        public void ShowInfo(string? message = null)
        {
            Message = message ?? Resources.messaging_service_default_info;
            CurrentType = MessageType.Info;
            OnMessageChanged?.Invoke();
        }

        public void ShowError(string? message = null)
        {
            Message = message ?? Resources.messaging_service_default_error;
            CurrentType = MessageType.Error;
            OnMessageChanged?.Invoke();
        }

        public void Clear()
        {
            Message = null;
            CurrentType = MessageType.None;
            OnMessageChanged?.Invoke();
        }

        public void ShowErrorDialog(string message)
        {
            _onConfirm = null;
            confirmDialog.ShowError(message);
        }

        public void ShowDialog(string message, Func<Task> onConfirm)
        {
            _onConfirm = onConfirm;
            confirmDialog.Show(message);
        }

        public async void OnDialogClosed(bool confirmed)
        {
            if (confirmed && _onConfirm != null)
            {
                await _onConfirm.Invoke();
                return;
            }
            else
            {
                return;
            }
        }
    }
}
