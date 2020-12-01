using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{

    public class MmsCategoriesRecord : BaseEntity
    {
        public virtual int MmsCategoriesID { get; set; }
        public virtual string MmsCategory { get; set; }
        public virtual bool IsNewCategory { get; set; }
        public virtual int MmsItemRecordId { get; set; }

//        public MmsItemRecord MmsItem { get; set; }
    }
}
