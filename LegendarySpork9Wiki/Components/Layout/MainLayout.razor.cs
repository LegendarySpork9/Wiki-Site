using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        [Inject]
        private APIService APIService { get; set; } = default!;

        private bool _jsAvailable;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _jsAvailable = true;
                User.OnDarkModeChanged += StateHasChanged;

                try
                {
                    bool darkMode = await JS.InvokeAsync<bool>("themeInterop.getTheme");
                    User.DarkMode = darkMode;
                    StateHasChanged();
                }
                catch
                {
                    // JS interop not available during prerender
                }
            }
        }

        private async Task ToggleDarkMode()
        {
            User.DarkMode = !User.DarkMode;

            if (_jsAvailable)
            {
                try
                {
                    await JS.InvokeVoidAsync("themeInterop.setTheme", User.DarkMode);
                }
                catch
                {
                    // JS interop not available
                }
            }

            if (User.IsLoggedIn)
            {
                await APIService.UpdateUserSettings(User.UserId, User.DarkMode);
            }
        }

        public void Dispose()
        {
            User.OnDarkModeChanged -= StateHasChanged;
        }
    }
}
