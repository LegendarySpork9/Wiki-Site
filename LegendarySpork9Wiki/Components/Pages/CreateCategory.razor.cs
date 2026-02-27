using Microsoft.AspNetCore.Components;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages
{
    public partial class CreateCategory : IDisposable
    {
        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        private List<CategoryModel>? _categories;
        private List<CategoryModel> _flatCategories = new();
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _icon = string.Empty;
        private string _selectedParentId = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _saving;

        protected override async Task OnInitializedAsync()
        {
            User.OnDarkModeChanged += StateHasChanged;
            _categories = await APIService.GetCategories();
            _flatCategories = FlattenCategories(_categories);
        }

        private List<CategoryModel> FlattenCategories(List<CategoryModel> categories, string prefix = "")
        {
            var result = new List<CategoryModel>();

            foreach (var cat in categories)
            {
                result.Add(new CategoryModel
                {
                    Id = cat.Id,
                    Name = string.IsNullOrEmpty(prefix) ? cat.Name : $"{prefix} > {cat.Name}",
                    Description = cat.Description
                });

                if (cat.SubCategories.Any())
                {
                    string nextPrefix = string.IsNullOrEmpty(prefix) ? cat.Name : $"{prefix} > {cat.Name}";
                    result.AddRange(FlattenCategories(cat.SubCategories, nextPrefix));
                }
            }

            return result;
        }

        private async Task HandleCreate()
        {
            _errorMessage = string.Empty;
            _successMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(_name))
            {
                _errorMessage = "Name is required.";
                return;
            }

            _saving = true;
            StateHasChanged();

            try
            {
                string? parentId = string.IsNullOrEmpty(_selectedParentId) ? null : _selectedParentId;

                var result = await APIService.CreateCategory(_name, _description, _icon, parentId);

                if (result != null)
                {
                    Navigation.NavigateTo($"/category/{result.Id}");
                }
                else
                {
                    _errorMessage = "Failed to create category. Please try again.";
                }
            }
            catch
            {
                _errorMessage = "An error occurred while creating the category.";
            }
            finally
            {
                _saving = false;
                StateHasChanged();
            }
        }

        public void Dispose()
        {
            User.OnDarkModeChanged -= StateHasChanged;
        }
    }
}
