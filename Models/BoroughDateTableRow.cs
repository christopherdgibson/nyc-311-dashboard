namespace NYC311Dashboard.Models
{
    public class BoroughDateTableRow
    {
        public string Borough { get; set; }
        public DateOnly? CreatedDate { get; set; }
        public int Count { get; set; }
        public double Duration { get; set; }
    }
}
