using NYC311Dashboard.Services.Contracts;

namespace NYC311Dashboard.Services
{
    public class LoadingService : ILoadingService
    {
        public event Action? OnLoadingChanged;

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnLoadingChanged?.Invoke();
                }
            }
        }

        public string LoadingMessage { get; set; } = Resources.loading_service_default_loading;
    }
}
