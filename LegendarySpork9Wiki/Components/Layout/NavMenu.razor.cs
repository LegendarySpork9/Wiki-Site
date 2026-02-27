using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        private List<CategoryModel>? _categories;
        private string? _currentCategoryId;

        protected override async Task OnInitializedAsync()
        {
            User.OnDarkModeChanged += StateHasChanged;
            Navigation.LocationChanged += OnLocationChanged;
            _categories = await APIService.GetCategories();
            UpdateCurrentCategoryId();
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            UpdateCurrentCategoryId();
            StateHasChanged();
        }

        private void UpdateCurrentCategoryId()
        {
            var uri = new Uri(Navigation.Uri);
            var segments = uri.AbsolutePath.Trim('/').Split('/');

            if (segments.Length >= 2 && segments[0].Equals("category", StringComparison.OrdinalIgnoreCase))
            {
                _currentCategoryId = segments[1];
            }
            else
            {
                _currentCategoryId = null;
            }
        }

        private bool IsCategoryActive(CategoryModel category)
        {
            if (_currentCategoryId == null) return false;
            if (category.Id == _currentCategoryId) return true;
            return ContainsCategory(category.SubCategories, _currentCategoryId);
        }

        private bool ContainsCategory(List<CategoryModel> categories, string id)
        {
            foreach (var cat in categories)
            {
                if (cat.Id == id) return true;
                if (ContainsCategory(cat.SubCategories, id)) return true;
            }

            return false;
        }

        public void Dispose()
        {
            User.OnDarkModeChanged -= StateHasChanged;
            Navigation.LocationChanged -= OnLocationChanged;
        }
    }
}
