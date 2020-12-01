using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{

    public class MmsTagsRecord : BaseEntity
    {
        public virtual int MmsTagsID { get; set; }
        public virtual string MmsTags { get; set; }
        public virtual int MmsItemRecordId { get; set; }

        public MmsItemRecord MmsItem { get; set; }
    }
}
