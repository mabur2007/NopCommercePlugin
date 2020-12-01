using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{

    public class MmsAlternateImage : BaseEntity
    {
        public virtual int MmsAlternateImageId { get; set; }

        public virtual string Name { get; set; }

        public virtual string DirectName { get; set; }

        public virtual string ThumbnailName { get; set; }

        public virtual int MmsItemRecordID { get; set; }
    }
}
