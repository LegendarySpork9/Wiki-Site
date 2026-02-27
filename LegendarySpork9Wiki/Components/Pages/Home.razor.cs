using Microsoft.AspNetCore.Components;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages
{
    public partial class Home : IDisposable
    {
        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        private List<EntryModel> _recentEntries = new();
        private bool _loading = true;

        protected override async Task OnInitializedAsync()
        {
            User.OnDarkModeChanged += StateHasChanged;
            _recentEntries = (await APIService.GetRecentEntries()).Take(6).ToList();
            _loading = false;
        }

        public void Dispose()
        {
            User.OnDarkModeChanged -= StateHasChanged;
        }
    }
}
