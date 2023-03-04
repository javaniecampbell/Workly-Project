using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workly.Core.ViewModels
{
    public class CommitViewModel
    {
        public int? AddedFileChange { get; set; }
        public int? EditedFileChange { get; set; }
        public int? DeletedFileChange { get; set; }
    }
}
