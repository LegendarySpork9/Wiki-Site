using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
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

        [Inject]
        private ProtectedSessionStorage SessionStorage { get; set; } = default!;

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
                }
                catch
                {
                    // JS interop not available during prerender
                }

                if (!User.IsLoggedIn)
                {
                    try
                    {
                        var userIdResult = await SessionStorage.GetAsync<string>("userId");

                        if (userIdResult.Success && !string.IsNullOrEmpty(userIdResult.Value))
                        {
                            var usernameResult = await SessionStorage.GetAsync<string>("username");
                            var adminResult = await SessionStorage.GetAsync<bool>("admin");

                            User.UserId = userIdResult.Value;
                            User.Username = usernameResult.Success ? usernameResult.Value! : string.Empty;
                            User.Admin = adminResult.Success && adminResult.Value;
                        }
                    }
                    catch
                    {
                        // Session storage not available
                    }
                }

                StateHasChanged();
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
