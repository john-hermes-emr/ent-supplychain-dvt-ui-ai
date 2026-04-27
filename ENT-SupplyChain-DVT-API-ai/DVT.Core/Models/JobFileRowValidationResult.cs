namespace DVT.Core.Models
{
    public class JobFileRowValidationResult
    {
        public int Row { get; set; }
        public string Status { get; set; }
        public List<JobFileRowColumnValidationResult> Columns { get; set; }
    }
}
