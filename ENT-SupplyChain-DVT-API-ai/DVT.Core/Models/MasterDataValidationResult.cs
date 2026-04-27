namespace DVT.Core.Models
{
    public class MasterDataValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> InvalidIds { get; set; }
    }
}
