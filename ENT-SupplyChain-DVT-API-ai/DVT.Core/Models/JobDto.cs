using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVT.Core.Models
{
    public class JobDto
    {
        public Guid JobId { get; set; }
        public Guid DivisionId { get; set; }
        public Guid UserInfoId { get; set; }
        public string Status { get; set; }
        public int FeedNumber { get; set; }
        public string ArchiveFilePath { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public List<JobFileDto> JobFiles { get; set; }
    }
}
