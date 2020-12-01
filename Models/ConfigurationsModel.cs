using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.MmsAdmin.Models
{
    public class ConfigurationsModel : BaseNopModel
    {
       
        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.DollarExchangeRate")]
        public decimal DollarExchangeRate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.ShippingMultiplyer")]
        public decimal ShippingMultiplyer { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.MarkupMultiplyerLower")]
        public decimal MarkupMultiplyerLower { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.MarkupMultiplyerUpper")]
        public decimal MarkupMultiplyerUpper { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.MarkupMutiplyerThreshold")]
        public decimal MarkupMutiplyerThreshold { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.LoadFromDate")]
        public DateTime LoadFromDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.LoadUntilDate")]
        public DateTime LoadUntilDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.LastLoadedDate")]
        public DateTime LastLoadedDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.MMSusername")]
        public string MMSusername { get; set; }

        [NopResourceDisplayName("Plugins.Misc.MmsAdmin.Fields.MMSpassword")]
        public string MMSpassword { get; set; }

    }
}
