namespace DVT.Core.Models
{
    public class JobValidation
    {
        public Guid JobId { get; set; }
        public List<Guid> SelectedFileIds { get; set; } = new List<Guid>();
    }
}
