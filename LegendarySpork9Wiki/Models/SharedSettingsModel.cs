namespace LegendarySpork9Wiki.Models
{
    public class SharedSettingsModel
    {
        public string Domain { get; set; } = string.Empty;
        public string BaseURL { get; set; } = string.Empty;
        public string Credentials { get; set; } = string.Empty;
        public string Endpoints { get; set; } = string.Empty;
        public string PayloadLocation { get; set; } = "Payload";
        public string RefreshTime { get; set; } = "30";
    }
}
