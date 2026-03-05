using NYC311Dashboard.Models.Contracts;

namespace NYC311Dashboard.Models
{
    public class RequestModelBase : IRequestModelBase
    {
        public string Borough { get; set; }
        public DateTime? CreatedDate { get; set; }
        public double Duration { get; set; }
    }
}
