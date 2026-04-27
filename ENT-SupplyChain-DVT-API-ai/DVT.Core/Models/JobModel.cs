namespace DVT.Core.Models
{
    public class JobModel
    {
        public Guid JobId { get; set; }
        public Guid DivisionId { get; set; }
        public int FeedNumber { get; set; }
        public Guid UserInfoId { get; set; }
        public string UpdateBy { get; set; }
        public string ArchiveFilePath { get; set; }
        public string Status { get; set; }
        public string JobLog { get; set; } //JSON string representation of job log    

        public List<IJobFileModel> JobFiles { get; set; }

        public IJobFileModel GetJobFileByFileType(string fileType)
        {
            if (JobFiles == null || JobFiles.Count == 0)
                return null;

            return JobFiles.FirstOrDefault(x => string.Equals(x.FileType, fileType));
        }
    }
}
