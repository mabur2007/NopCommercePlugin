using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{

    public class MmsRelatedProductsRecord : BaseEntity
    {
        public virtual int MmsRelatedProductsID { get; set; }
        public virtual int MmsRelatedItemRecord { get; set; }
        public virtual int MmsItemRecordId { get; set; }

        public MmsItemRecord MmsItem { get; set; }
    }
}
