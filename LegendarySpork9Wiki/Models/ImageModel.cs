namespace LegendarySpork9Wiki.Models
{
    public class ImageModel
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateUploaded { get; set; }
    }
}
