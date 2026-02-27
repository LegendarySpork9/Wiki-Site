namespace LegendarySpork9Wiki.Models
{
    public class APIEntriesModel
    {
        public List<EntryModel> Entries { get; set; } = new List<EntryModel>();
        public bool MultiplePages { get; set; }
        public int PageCount { get; set; }
        public bool APICalled { get; set; }
    }
}
