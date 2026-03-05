namespace NYC311Dashboard.Services.Models
{
    public class BoroughZipSelection
    {
        public string Borough { get; init; }
        public List<string> AvailableZips { get; set; } = new();
        public HashSet<string> SelectedZips { get; set; } = new();

        public BoroughZipSelection(string borough, List<string> availableZips)
        {
            Borough = borough;
            AvailableZips = availableZips;
            SelectedZips = availableZips.ToHashSet();  // default all selected
        }
    }
}
