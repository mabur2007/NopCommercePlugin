using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{

    public class MmsSalesOrderItemRecord : BaseEntity
    {
        public virtual int MmsSalesOrderItemRecordId { get; set; }

        public virtual int InternalId { get; set; }

        public virtual string Description { get; set; }

        public virtual int InternalIdLegacy { get; set; }

        public virtual int Quantity { get; set; }

        public virtual double Price { get; set; }

        public virtual int MmsSalesOrderRecordId { get; set; }
        public MmsSalesOrderRecord MmsSalesOrderRecord { get; set; }

    }
}
