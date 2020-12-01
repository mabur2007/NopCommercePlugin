using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MmsAdmin.Models
{
    public class MmsJsonGetDownloadLinkv2
    {
        public int PercentComplete { get; set; }
        public string RequestTime { get; set; }
        public string Status { get; set; }
        public string DownloadLink { get; set; }
    }
}
