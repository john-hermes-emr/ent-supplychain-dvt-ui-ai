namespace DVT.Core.Models
{
    public interface IJobFileModel
    {
        public Guid JobFileId { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public bool IsSelected { get; set; }
        public List<string> FileHeader { get; set; }
        public List<IDataRow> DataRows { get; set; }
    }
}
