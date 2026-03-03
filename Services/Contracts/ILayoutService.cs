using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

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

        RenderFragment RenderSidebarButton(string buttonText, string classes, string message, Func<Task>? onConfirm);

        RenderFragment RenderInactiveSidebarButton(string buttonText, string message);

        RenderFragment RenderCustomSidebar<TItem>(
            string label,
            IEnumerable<TItem> options,
            HashSet<TItem> selectedValues,
            EventCallback<HashSet<TItem>> selectedValuesChanged,
            Func<Task>? onSelectionChanged,
            bool inactive = false,
            Func<TItem, string>? optionLabel = null);

        Task ScrollToTop();

        Task ChangeClassName(string oldClassName, string newClassName);

        Task ToggleClassName(string element = "nav ul", string newClassName = "nav-open");

        Task CloseNavOnClick(string element = "nav ul", string newClassName = "nav-open");
    }
}
