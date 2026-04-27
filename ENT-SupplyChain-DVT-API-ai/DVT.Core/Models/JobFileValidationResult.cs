namespace DVT.Core.Models
{
    /// <summary>
    /// User Story 16177067: 7 - Validation Service - Validation Message Structure
    /// </summary>
    public class JobFileValidationResult
    {
        public string FileName { get; set; }
        public List<JobFileRowValidationResult> ValidationRows { get; set; }
    }
}
