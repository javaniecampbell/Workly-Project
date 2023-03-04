using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workly.Core.Models
{
    public class Commit
    {
        public Commit()
        {
            CommitId = string.Empty;
            Subject= string.Empty;
            CommitMessage= string.Empty;
            Body= null;
            CommitterEmail= null;
            CommitterDate = null;
            CommitterName = null;
            FileChangeAdded= null;
            FileChangeEdited= null;
            FileChangeDeleted= null;
        }
        public string CommitId { get; set; }
        public string Subject { get; set; }
        public string CommitMessage { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Body { get; set; }
        public string CommitterName { get; set; }
        public DateTime? CommitterDate { get; set; }
        public string CommitterEmail { get; set;}

        public int? FileChangeAdded { get; set; }
        public int? FileChangeEdited { get; set; }
        public int? FileChangeDeleted { get; set;}

    }
}
