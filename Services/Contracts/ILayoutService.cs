using Microsoft.AspNetCore.Components;
using NYC311Dashboard.Services.Models;

namespace NYC311Dashboard.Services.Contracts
{
    public interface ILayoutService
    {
        string? MainTitle { get; }
        string? SupTitle { get; }

        RenderFragment? CustomSidebar { get; }

        event Action? OnSidebarChanged;

        event Action? OnLocationChanged;

        void SetTitle(string? mainTitle, string? supTitle = null);

        void SetSidebar(RenderFragment? fragment);

        RenderFragment RenderButton(string buttonText, string classes, string message, Func<Task>? onConfirm);

        RenderFragment RenderInactiveButton(string buttonText, string message);

        public RenderFragment RenderCheckboxDropdown<TItem>(CheckboxDropdownConfig<TItem> config, string header)
        {
            return RenderMultipleCheckboxDropdowns(new List<CheckboxDropdownConfig<TItem>> { config }, header);
        }
         
        RenderFragment RenderMultipleCheckboxDropdowns<TItem>(IEnumerable<CheckboxDropdownConfig<TItem>> configs, string header);

        Task ScrollToTop();

        Task ChangeClassName(string oldClassName, string newClassName);

        Task ToggleClassName(string element = "nav ul", string newClassName = "nav-open");

        Task CloseNavOnClick(string element = "nav ul", string newClassName = "nav-open");
    }
}
