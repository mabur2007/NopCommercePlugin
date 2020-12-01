using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MmsAdmin.Models
{
    public class MmsJsonGetDownload
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TimeAdded { get; set; }
        public string CreatorName { get; set; }
        public int Price { get; set; }
        public string Type { get; set; }
        public string LiveStreamStartTime { get; set; }
        public int NumberOfVideos { get; set; }
        public bool CanDownload { get; set; }
        public string Status { get; set; }
        public int MinutesTillLecture { get; set; }
    }
}
