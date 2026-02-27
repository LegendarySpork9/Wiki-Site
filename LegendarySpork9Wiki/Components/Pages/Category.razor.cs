using Microsoft.AspNetCore.Components;
using LegendarySpork9Wiki.Models;
using LegendarySpork9Wiki.Services;

namespace LegendarySpork9Wiki.Components.Pages
{
    public partial class Category : IDisposable
    {
        [Parameter]
        public string CategoryId { get; set; } = string.Empty;

        [Inject]
        private APIService APIService { get; set; } = default!;

        [Inject]
        private UserModel User { get; set; } = default!;

        private CategoryModel? _category;
        private CategoryModel? _rootCategory;
        private List<CategoryModel>? _allCategories;
        private APIEntriesModel? _entries;
        private bool _loading = true;
        private int _currentPage = 1;

        protected override async Task OnInitializedAsync()
        {
            User.OnDarkModeChanged += StateHasChanged;
            await LoadData();
        }

        protected override async Task OnParametersSetAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            _loading = true;
            _currentPage = 1;

            _allCategories = await APIService.GetCategories();
            _category = FindCategory(_allCategories, CategoryId);
            _rootCategory = FindRootCategory(_allCategories, CategoryId);
            _entries = await APIService.GetEntries(CategoryId, _currentPage);

            _loading = false;
        }

        private CategoryModel? FindRootCategory(List<CategoryModel> categories, string id)
        {
            foreach (var cat in categories)
            {
                if (cat.Id == id || ContainsCategory(cat.SubCategories, id))
                {
                    return cat;
                }
            }

            return null;
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

        private CategoryModel? FindCategory(List<CategoryModel> categories, string id)
        {
            foreach (var cat in categories)
            {
                if (cat.Id == id) return cat;

                var found = FindCategory(cat.SubCategories, id);
                if (found != null) return found;
            }

            return null;
        }

        private async Task LoadPage(int pageNumber)
        {
            _currentPage = pageNumber;
            _entries = await APIService.GetEntries(CategoryId, _currentPage);
            StateHasChanged();
        }

        public void Dispose()
        {
            User.OnDarkModeChanged -= StateHasChanged;
        }
    }
}
