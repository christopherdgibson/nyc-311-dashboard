namespace NYC311Dashboard.Models
{
    public class ZipHourTableRow
    {
        public string Borough { get; set; }
        public string Zip { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int Count { get; set; }
        public double Duration { get; set; }
    }
}
