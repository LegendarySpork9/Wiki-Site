namespace LegendarySpork9Wiki.Models
{
    public class APIImagesModel
    {
        public List<ImageModel> Images { get; set; } = new List<ImageModel>();
        public bool MultiplePages { get; set; }
        public int PageCount { get; set; }
        public bool APICalled { get; set; }
    }
}
