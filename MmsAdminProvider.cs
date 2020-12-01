using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Plugin.Misc.MmsAdmin;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.MmsAdmin
{
    public class MmsAdminProvider : BasePlugin, IAdminMenuPlugin
    {

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;

        public MmsAdminProvider(
            ILocalizationService localizationService,
            IWebHelper webHelper,
                       ISettingService settingService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/MmsAdmin/Configure";
        }


        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            var settings = new MMSSettings
            {
                DollarExchangeRate = 1,
                ShippingMultiplyer = 1,
                MarkupMultiplyerLower = 1,
                MarkupMultiplyerUpper = 1,
                MarkupMutiplyerThreshold = 10,
                LoadFromDate = DateTime.Now,
                LoadUntilDate = DateTime.Now,
                LastLoadedDate = DateTime.Now,
                MMSusername = "noddy@bigears.com",
                MMSpassword = "noddyandbigears"
            };
            _settingService.SaveSetting(settings);

            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Misc.MmsAdmin.Fields.DollarExchangeRate"] = "Dollar Exchange Rate",
                ["Plugins.Misc.MmsAdmin.Fields.ShippingMultiplyer"] = "Shipping Multiplyer",
                ["Plugins.Misc.MmsAdmin.Fields.MarkupMultiplyerLower"] = "Markup Multiplyer Lower",
                ["Plugins.Misc.MmsAdmin.Fields.MarkupMultiplyerUpper"] = "Markup Multiplyer Upper",
                ["Plugins.Misc.MmsAdmin.Fields.MarkupMutiplyerThreshold"] = "Markup Mutiplyer Threshold",
                ["Plugins.Misc.MmsAdmin.Fields.LoadFromDate"] = "Load From Date",
                ["Plugins.Misc.MmsAdmin.Fields.LoadUntilDate"] = "Load Until Date",
                ["Plugins.Misc.MmsAdmin.Fields.LastLoadedDate"] = "Last Loaded Date",
                ["Plugins.Misc.MmsAdmin.Fields.MMSusername"] = "MMS Username",
                ["Plugins.Misc.MmsAdmin.Fields.MMSpassword"] = "MMS Password"
            });
            base.Install();
            Debug.WriteLine(">>>>>>>>>>>  In Install Line 66");

        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Misc.MmsAdmin");

            base.Uninstall();
        }


        public void ManageSiteMap(SiteMapNode rootNode)
        {
            Debug.WriteLine(">>>In MmsAdminNop2 Line 27 >>>>");

            var menuItem = new SiteMapNode()
            {
                SystemName = "Misc.MmsAdmin",
                Title = "Load Latest MMSAdmin !!>>",
                ControllerName = "MmsAdmin",
                ActionName = "MmsDataLoad",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
            };
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);

            var menuItem2 = new SiteMapNode()
            {
                SystemName = "Misc.MmsAdmin",
                Title = "Test Download MmsAdmin!! >>",
                ControllerName = "MmsAdmin",
                ActionName = "TestDownload",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
            };
            var pluginNode2 = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode2 != null)
                pluginNode2.ChildNodes.Add(menuItem2);
            else
                rootNode.ChildNodes.Add(menuItem2);
        }
    }
}