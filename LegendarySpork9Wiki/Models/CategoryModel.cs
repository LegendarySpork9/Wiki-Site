namespace LegendarySpork9Wiki.Models
{
    public class CategoryModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? ParentCategoryId { get; set; }
        public List<CategoryModel> SubCategories { get; set; } = new List<CategoryModel>();
    }
}
