namespace LegendarySpork9Wiki.Models
{
    public class UserModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Admin { get; set; }

        private bool _darkMode;
        public bool DarkMode
        {
            get => _darkMode;
            set
            {
                if (_darkMode != value)
                {
                    _darkMode = value;
                    OnDarkModeChanged?.Invoke();
                }
            }
        }

        public event Action? OnDarkModeChanged;

        public bool IsLoggedIn => !string.IsNullOrEmpty(UserId);
    }
}
