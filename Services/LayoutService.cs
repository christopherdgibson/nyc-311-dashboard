using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using NYC311Dashboard.Components;
using NYC311Dashboard.Services.Contracts;
using System.Runtime.Versioning;

namespace NYC311Dashboard.Services
{
    public class LayoutService : ILayoutService, IDisposable
    {
        private readonly NavigationManager _navigation;
        private readonly IJSRuntime _js;
        private readonly ILoadingService _loadingService;
        private readonly IMessagingService _messagingService;

        public LayoutService(NavigationManager navigation, IJSRuntime js, ILoadingService loadingService, IMessagingService messagingService)
        {
            _navigation = navigation;
            _js = js;
            _loadingService = loadingService;
            _messagingService = messagingService;

            _navigation.LocationChanged += OnNavigationChanged;
        }

        public string? MainTitle { get; private set; }
        public string? SupTitle { get; private set; }

        public RenderFragment? CustomSidebar { get; private set; }
        public event Action? OnSidebarChanged;
        public event Action? OnLocationChanged;

        public void SetTitle(string? mainTitle, string? supTitle = null)
        {
            MainTitle = mainTitle;
            SupTitle = supTitle;
            OnSidebarChanged?.Invoke(); // reuse existing event
        }

        public void SetSidebar(RenderFragment? fragment)
        {
            CustomSidebar = fragment;
            OnSidebarChanged?.Invoke();
        }
        public RenderFragment RenderInactiveSidebarButton(string buttonText, string message) => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "sidebar-btn");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () => _messagingService.ShowErrorDialog(message)));
            builder.AddContent(3, buttonText);
            builder.CloseElement();
        };

        public RenderFragment RenderSidebarButton(string buttonText, string classes, string message, Func<Task>? onConfirm) => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", classes);
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () => _messagingService.ShowDialog(message, onConfirm)));
            builder.AddContent(3, buttonText);
            builder.CloseElement();
        };

        public RenderFragment RenderCustomSidebar<TItem>(
            string label,
            IEnumerable<TItem> options,
            HashSet<TItem> selectedValues,
            EventCallback<HashSet<TItem>> selectedValuesChanged,
            Func<Task>? onSelectionChanged,
            bool inactive = false,
            Func<TItem, string>? optionLabel = null)
        {
            _loadingService.LoadingMessage = Resources.loading_service_loading_here;
            _loadingService.IsLoading = true;
            try
            {
                RenderFragment? customSidebar = builder =>
                {
                    builder.OpenComponent(0, typeof(CheckboxDropdown<TItem>));
                    builder.AddAttribute(1, "Label", label);
                    builder.AddAttribute(2, "Options", options);
                    builder.AddAttribute(3, "SelectedValues", selectedValues);
                    builder.AddAttribute(4, "SelectedValuesChanged", selectedValuesChanged);
                    builder.AddAttribute(5, "OnSelectionChanged", onSelectionChanged);
                    builder.AddAttribute(6, "OptionLabel", optionLabel ?? (x => x?.ToString()));
                    builder.AddAttribute(7, "SetIndeterminateSelection", SetIndeterminate);
                    builder.CloseComponent();
                };

                CustomSidebar = customSidebar;

                return customSidebar;
            }
            catch
            {
                _messagingService.ShowError(Resources.messaging_service_error_occurred);
                //_messagingService.ShowError(Resources.messaging_service_error_occurred);
                return CustomSidebar; // Result.Failure(Resources.messaging_service_error_occurred);
            }
            finally
            {
                _loadingService.IsLoading = false;
            }
        }

        private async void OnNavigationChanged(object? sender, LocationChangedEventArgs e)
        {
            try
            {
                //SetTitle(null); if needed later
                await ScrollToTop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Navigation handler failed: {ex.Message}");
            }

            OnLocationChanged?.Invoke();
        }

        private async Task SetIndeterminate(ElementReference selectAllRef, bool IsIndeterminate)
        {
            await _js.InvokeVoidAsync("setIndeterminateSelection", selectAllRef, IsIndeterminate);
        }

        public async Task ChangeClassName(string oldClassName, string newClassName)
        {
            await _js.InvokeVoidAsync("changeClassName", oldClassName, newClassName);
        }

        public async Task ToggleClassName(string element = "nav ul", string className = "nav-open")
        {
            await _js.InvokeVoidAsync("toggleNav", element, className);
        }

        public async Task CloseNavOnClick(string element = "nav ul", string newClassName = "nav-open")
        {
            await _js.InvokeVoidAsync("closeNavOnClick");
        }

        public async Task ScrollToTop()
        {
            await _js.InvokeVoidAsync("scrollToTop");
        }
        public void Dispose()
        {
            _navigation.LocationChanged -= OnNavigationChanged;
        }
    }
}
