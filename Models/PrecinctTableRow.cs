namespace NYC311Dashboard.Models
{
    public class PrecinctTableRow
    {
        public string Borough { get; set; }
        public string Precinct { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int Count { get; set; }
        public double Duration { get; set; }
    }
}
