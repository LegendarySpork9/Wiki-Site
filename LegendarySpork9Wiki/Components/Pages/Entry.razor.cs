using Microsoft.AspNetCore.Components;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages
{
    public partial class Entry : IDisposable
    {
        [Parameter]
        public string EntryId { get; set; } = string.Empty;

        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        private EntryModel? _entry;
        private bool _loading = true;

        protected override async Task OnInitializedAsync()
        {
            User.OnDarkModeChanged += StateHasChanged;
            await LoadEntry();
        }

        protected override async Task OnParametersSetAsync()
        {
            await LoadEntry();
        }

        private async Task LoadEntry()
        {
            _loading = true;
            _entry = await APIService.GetEntry(EntryId);
            _loading = false;
        }

        public void Dispose()
        {
            User.OnDarkModeChanged -= StateHasChanged;
        }
    }
}
