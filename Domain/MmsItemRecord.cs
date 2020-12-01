using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.MmsAdmin.Domain
{

    public class MmsItemRecord : BaseEntity
    {

        private ICollection<MmsAlternateImage> _mms_alternate_image;

        public virtual ICollection<MmsAlternateImage> AlternateImages
        {
            get { return _mms_alternate_image ?? (_mms_alternate_image = new List<MmsAlternateImage>()); }
            protected set { _mms_alternate_image = value; }
        }

        private ICollection<MmsVideo> _mms_video;

        public virtual ICollection<MmsVideo> Videos
        {
            get { return _mms_video ?? (_mms_video = new List<MmsVideo>()); }
            protected set { _mms_video = value; }
        }

        private ICollection<MmsWholesalePriceLevel> _mms_wholesale_price_level;

        public virtual ICollection<MmsWholesalePriceLevel> MmsWholesalePriceLevels
        {
            get { return _mms_wholesale_price_level ?? (_mms_wholesale_price_level = new List<MmsWholesalePriceLevel>()); }
            protected set { _mms_wholesale_price_level = value; }
        }

        private ICollection<MmsCategoriesRecord> _mms_category_record;

        public virtual ICollection<MmsCategoriesRecord> MmsCategories
        {
            get { return _mms_category_record ?? (_mms_category_record = new List<MmsCategoriesRecord>()); }
            protected set { _mms_category_record = value; }
        }

        private ICollection<MmsRateRecord> _mms_rate_record;

        public virtual ICollection<MmsRateRecord> MmsRates
        {
            get { return _mms_rate_record ?? (_mms_rate_record = new List<MmsRateRecord>()); }
            protected set { _mms_rate_record = value; }
        }

        private ICollection<MmsRelatedProductsRecord> _mms_related_product_record;

        public virtual ICollection<MmsRelatedProductsRecord> MmsRelatedProduct
        {
            get { return _mms_related_product_record ?? (_mms_related_product_record = new List<MmsRelatedProductsRecord>()); }
            protected set { _mms_related_product_record = value; }
        }

        private ICollection<MmsTagsRecord> _mms_tags_record;

        public virtual ICollection<MmsTagsRecord> MmsTags
        {
            get { return _mms_tags_record ?? (_mms_tags_record = new List<MmsTagsRecord>()); }
            protected set { _mms_tags_record = value; }
        }

        public virtual int MmsItemRecordID { get; set; }
        public virtual int InternalId { get; set; }
        public virtual int ProductId { get; set; }
        public virtual bool IsMmsDownload { get; set; }
        //       public virtual MmsAlternateImage[] AlternateImages { get; set; }
        public virtual string ArtistOrMagician { get; set; }
        //       public virtual string[] CategoriesNew { get; set; }
        //       public virtual string[] Categories { get; set; }
        public virtual System.DateTime DateAdded { get; set; }
        public virtual System.DateTime DateLastModified { get; set; }
        public virtual string HTMLDescription { get; set; }
        public virtual string ImageFileName { get; set; }
        public virtual string ImageThumbnailFileName { get; set; }
        public virtual int InternalIdLegacy { get; set; }
        public virtual bool IsMurphysItem { get; set; }
        public virtual string ISBN { get; set; }
        public virtual bool MaintainMSRP { get; set; }
        public virtual string Manufacturer { get; set; }
        public virtual bool PreSale { get; set; }
        public virtual string ProductCode { get; set; }
        public virtual string ProductLine { get; set; }
        public virtual string Quality { get; set; }
        //        public virtual string[] Rating { get; set; }
        //        public virtual int[] RelatedProducts { get; set; }
        public virtual int QuantityAvailable { get; set; }
        public virtual int Status { get; set; }
        public virtual decimal SuggestedRetailPrice { get; set; }
        //        public virtual string[] Tags { get; set; }
        public virtual string Title { get; set; }
        public virtual double Weight { get; set; }
        public virtual decimal WholesalePrice { get; set; }
        //        public virtual MmsVideo[] Videos { get; set; }
        //        public virtual int[] WholesalePriceLevels { get; set; }
        public virtual decimal Length { get; set; }
        public virtual decimal Width { get; set; }
        public virtual decimal Height { get; set; }

        //public Product NopProductId {get; set; }
    }


}
