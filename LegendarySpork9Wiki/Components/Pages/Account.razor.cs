using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using LegendarySpork9Wiki.Functions;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages
{
    public partial class Account : IDisposable
    {
        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;
        private bool _isError;
        private bool _saving;

        protected override void OnInitialized()
        {
            User.OnDarkModeChanged += StateHasChanged;

            if (User.IsLoggedIn)
            {
                _username = User.Username;
            }
        }

        private async Task ToggleDarkMode(ChangeEventArgs e)
        {
            User.DarkMode = (bool)(e.Value ?? false);

            try
            {
                await JS.InvokeVoidAsync("themeInterop.setTheme", User.DarkMode);
            }
            catch
            {
                // JS interop not available
            }

            if (User.IsLoggedIn)
            {
                await APIService.UpdateUserSettings(User.UserId, User.DarkMode);
            }
        }

        private async Task SaveSettings()
        {
            _message = string.Empty;
            _saving = true;
            StateHasChanged();

            try
            {
                if (string.IsNullOrWhiteSpace(_username))
                {
                    _message = "Username cannot be empty.";
                    _isError = true;
                    return;
                }

                string passwordHash = string.IsNullOrWhiteSpace(_password)
                    ? string.Empty
                    : HashFunction.ComputeSHA512(_password);

                bool success = await APIService.UpdateUser(User.UserId, _username, passwordHash);

                if (success)
                {
                    User.Username = _username;
                    _password = string.Empty;
                    _message = "Settings saved successfully.";
                    _isError = false;
                }
                else
                {
                    _message = "Failed to save settings. Please try again.";
                    _isError = true;
                }
            }
            catch
            {
                _message = "An error occurred while saving settings.";
                _isError = true;
            }
            finally
            {
                _saving = false;
                StateHasChanged();
            }
        }

        private void Logout()
        {
            User.UserId = string.Empty;
            User.Username = string.Empty;
            User.Admin = false;
            Navigation.NavigateTo("/");
        }

        public void Dispose()
        {
            User.OnDarkModeChanged -= StateHasChanged;
        }
    }
}
