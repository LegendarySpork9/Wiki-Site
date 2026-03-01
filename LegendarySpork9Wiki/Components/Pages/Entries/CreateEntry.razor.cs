using Microsoft.AspNetCore.Components;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages.Entries
{
    public partial class CreateEntry : IDisposable
    {
        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private TemplateService TemplateService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        [SupplyParameterFromQuery]
        public string? CategoryId { get; set; }

        private List<CategoryModel>? _categories;
        private List<CategoryModel> _flatCategories = new();
        private List<string> _templateNames = new();
        private string _selectedTemplate = string.Empty;
        private string _title = string.Empty;
        private string _selectedCategoryId = string.Empty;
        private string _summary = string.Empty;
        private string _content = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _saving;
        private bool _previewing;

        protected override async Task OnInitializedAsync()
        {
            User.OnDarkModeChanged += StateHasChanged;
            _categories = await APIService.GetCategories();
            _flatCategories = FlattenCategories(_categories);
            _templateNames = TemplateService.GetTemplateNames();

            if (!string.IsNullOrEmpty(CategoryId))
            {
                _selectedCategoryId = CategoryId;
            }
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

        private async Task OnTemplateSelected(ChangeEventArgs e)
        {
            _selectedTemplate = e.Value?.ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(_selectedTemplate))
            {
                _content = await TemplateService.GetTemplateContent(_selectedTemplate);
            }
        }

        private async Task HandleCreate()
        {
            _errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(_title))
            {
                _errorMessage = "Title is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedCategoryId))
            {
                _errorMessage = "Please select a category.";
                return;
            }

            _saving = true;
            StateHasChanged();

            try
            {
                var newEntry = new APINewEntryModel
                {
                    Title = _title,
                    Content = _content,
                    CategoryId = _selectedCategoryId,
                    Author = User.Username,
                    Summary = _summary
                };

                var result = await APIService.CreateEntry(newEntry);

                if (result != null)
                {
                    Navigation.NavigateTo($"/entry/{result.Id}");
                }
                else
                {
                    _errorMessage = "Failed to create entry. Please try again.";
                }
            }
            catch
            {
                _errorMessage = "An error occurred while creating the entry.";
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
