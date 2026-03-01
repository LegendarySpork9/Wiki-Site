using Microsoft.AspNetCore.Components;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages.Entries
{
    public partial class EditEntry : IDisposable
    {
        [SupplyParameterFromQuery]
        public string? EntryId { get; set; }

        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        private EntryModel? _entry;
        private List<CategoryModel> _flatCategories = new();
        private string _title = string.Empty;
        private string _selectedCategoryId = string.Empty;
        private string _summary = string.Empty;
        private string _content = string.Empty;
        private string _message = string.Empty;
        private bool _isError;
        private bool _loading = true;
        private bool _saving;
        private bool _previewing;

        protected override async Task OnInitializedAsync()
        {
            User.OnDarkModeChanged += StateHasChanged;

            if (!string.IsNullOrEmpty(EntryId))
            {
                var categories = await APIService.GetCategories();
                _flatCategories = FlattenCategories(categories);

                _entry = await APIService.GetEntry(EntryId);

                if (_entry != null)
                {
                    _title = _entry.Title;
                    _selectedCategoryId = _entry.CategoryId;
                    _summary = _entry.Summary;
                    _content = _entry.Content;
                }
            }

            _loading = false;
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

        private async Task HandleSave()
        {
            _message = string.Empty;

            if (string.IsNullOrWhiteSpace(_title))
            {
                _message = "Title is required.";
                _isError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedCategoryId))
            {
                _message = "Please select a category.";
                _isError = true;
                return;
            }

            _saving = true;
            StateHasChanged();

            try
            {
                var updatedEntry = new APINewEntryModel
                {
                    Title = _title,
                    Content = _content,
                    CategoryId = _selectedCategoryId,
                    Author = User.Username,
                    Summary = _summary
                };

                bool success = await APIService.UpdateEntry(EntryId!, updatedEntry);

                if (success)
                {
                    Navigation.NavigateTo($"/entry/{EntryId}");
                }
                else
                {
                    _message = "Failed to save changes. Please try again.";
                    _isError = true;
                }
            }
            catch
            {
                _message = "An error occurred while saving the entry.";
                _isError = true;
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
