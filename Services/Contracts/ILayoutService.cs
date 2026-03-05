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

        RenderFragment RenderCheckboxDropdown<TItem>(
            string label,
            IEnumerable<TItem> options,
            HashSet<TItem> selectedValues,
            EventCallback<HashSet<TItem>> selectedValuesChanged,
            Func<Task>? onSelectionChanged,
            bool inactive = false,
            Func<TItem, string>? optionLabel = null);

        RenderFragment RenderMultipleCheckboxDropdowns<TItem>(IEnumerable<CheckboxDropdownConfig<TItem>> configs);

        Task ScrollToTop();

        Task ChangeClassName(string oldClassName, string newClassName);

        Task ToggleClassName(string element = "nav ul", string newClassName = "nav-open");

        Task CloseNavOnClick(string element = "nav ul", string newClassName = "nav-open");
    }
}
