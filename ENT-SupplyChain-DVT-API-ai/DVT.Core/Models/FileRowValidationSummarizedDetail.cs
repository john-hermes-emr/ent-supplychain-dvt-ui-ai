namespace DVT.Core.Models
{
    public class FileRowValidationSummarizedDetail
    {
        public Guid GroupId { get; set; }
        public string RowNumber { get; set; }
        public string Problem { get; set; }
        public string ErrorDescription { get; set; }
        public string Data { get; set; }
        //public string Reference { get; set; }
    }
}
