namespace DVT.Core.Models
{
    public class FileLoadResult
    {
        public Guid JobFileId { get; set; }
        public string Operation { get; set; }
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public List<string> FileHeader { get; set; }
        public List<IDataRow> DataRows { get; set; }
    }
}
