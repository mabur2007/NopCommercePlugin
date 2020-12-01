using Nop.Core.Configuration;
using System;

namespace Nop.Plugin.Misc.MmsAdmin
{
    /// <summary>
    /// Represents settings of the UPS shipping plugin
    /// </summary>
    public class MMSSettings : ISettings
    {
        public decimal DollarExchangeRate { get; set; }
        public decimal ShippingMultiplyer { get; set; }
        public decimal MarkupMultiplyerLower { get; set; }
        public decimal MarkupMultiplyerUpper { get; set; }
        public decimal MarkupMutiplyerThreshold { get; set; }
        public DateTime LoadFromDate { get; set; }
        public DateTime LoadUntilDate { get; set; }
        public DateTime LastLoadedDate { get; set; }
        public string MMSusername { get; set; }
        public string MMSpassword { get; set; }
    }
}