using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{
    public class MmsWholesalePriceLevel : BaseEntity
    {
        public virtual int MmsWholesalePriceLevelId { get; set; }

        public virtual int Quantity { get; set; }

        public virtual decimal Price { get; set; }

        public virtual int MmsItemRecordID { get; set; }
        public MmsItemRecord MmsItem { get; set; }
    }

}
