using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{

    public class MmsSalesOrderRecord : BaseEntity
    {
        private ICollection<MmsSalesOrderItemRecord> _mms_sales_order_item_record;

        public virtual ICollection<MmsSalesOrderItemRecord> MmsSalesOrderItemRecords
        {
            get { return _mms_sales_order_item_record ?? (_mms_sales_order_item_record = new List<MmsSalesOrderItemRecord>()); }
            protected set { _mms_sales_order_item_record = value; }
        }

        public virtual int MmsSalesOrderRecordId { get; set; }

        public virtual bool IsSalesOrderOpen { get; set; }

        public virtual string SalesOrderNumber { get; set; }

        public virtual double Total { get; set; }

        //      public virtual MmsSalesOrderItemRecord[] Items { get; set; }

    }
}
