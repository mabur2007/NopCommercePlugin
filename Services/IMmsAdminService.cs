using Nop.Data;
using Nop.Plugin.Misc.MmsAdmin.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MmsAdmin.Services
{
    public partial interface IMmsAdminService
    {
        public IList<string> GetVideoByProductId(int productId);

        public int InsertVideo(int productId, string mms_video_url, int mms_item_id);

        public int UpdateVideo(MmsNopVideo video);

        //
        public int MmsGetMmsItemId(int nop_product_id);
        public int MmsGetNopProdId(int mms_item_id);
        public void MmsAddSOItems(int[] mms_item_ids, int[] mms_item_qtys);

    }
}
