using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{
    public class MmsVideo : BaseEntity
    {
        public virtual int MmsVideoId { get; set; }
        public virtual string Fillename { get; set; }
        public virtual int MmsItemRecordID { get; set; }
        public virtual string DirectFillename { get; set; }


        public MmsItemRecord MmsItem { get; set; }
    }

}
