using Microsoft.AspNetCore.Components;

namespace NYC311Dashboard.Services.Models
{
        public record CheckboxDropdownConfig<TItem>(
            string Label,
            IEnumerable<TItem> Options,
            HashSet<TItem>? SelectedValues,
            EventCallback<HashSet<TItem>> SelectedValuesChanged,
            Func<Task>? OnSelectionChanged,
            Func<TItem, string>? OptionLabel = null
        );
}

