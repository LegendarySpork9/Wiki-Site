using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LegendarySpork9Wiki.Functions;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages
{
    public partial class Login
    {
        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private ProtectedSessionStorage SessionStorage { get; set; } = default!;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _loading;

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await HandleLogin();
            }
        }

        private async Task HandleLogin()
        {
            _errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
            {
                _errorMessage = "Please enter both username and password.";
                return;
            }

            _loading = true;
            StateHasChanged();

            try
            {
                string passwordHash = HashFunction.ComputeSHA512(_password);
                var result = await APIService.AuthoriseAsync(_username, passwordHash);

                if (result != null)
                {
                    User.UserId = result.UserId;
                    User.Username = result.Username;
                    User.Admin = result.Admin;
                    User.DarkMode = result.DarkMode;

                    await SessionStorage.SetAsync("userId", User.UserId);
                    await SessionStorage.SetAsync("username", User.Username);
                    await SessionStorage.SetAsync("admin", User.Admin);

                    try
                    {
                        await JS.InvokeVoidAsync("themeInterop.setTheme", User.DarkMode);
                    }
                    catch
                    {
                        // JS interop not available
                    }

                    Navigation.NavigateTo("/");
                }
                else
                {
                    _errorMessage = "Invalid username or password.";
                }
            }
            catch
            {
                _errorMessage = "An error occurred during login. Please try again.";
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }
        }
    }
}
