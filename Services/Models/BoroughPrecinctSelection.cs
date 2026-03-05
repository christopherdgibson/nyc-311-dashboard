namespace NYC311Dashboard.Services.Models
{
    public class BoroughPrecinctSelection
    {
        public string Borough { get; init; }
        public List<string> AvailablePrecincts { get; set; } = new();
        public HashSet<string> SelectedPrecincts { get; set; } = new();

        public BoroughPrecinctSelection(string borough, List<string> availablePrecincts)
        {
            Borough = borough;
            AvailablePrecincts = availablePrecincts;
            SelectedPrecincts = availablePrecincts.ToHashSet();  // default all selected
        }
    }
}
