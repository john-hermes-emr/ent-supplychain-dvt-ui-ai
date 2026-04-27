namespace DVT.Api.Contracts.File
{
    public class FileLoadRequest
    {
        public string Template { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool Selected { get; set; }
        public List<string> FileContent { get; set; }
    }
}
