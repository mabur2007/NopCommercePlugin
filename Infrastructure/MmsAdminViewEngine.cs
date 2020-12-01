using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Web.Framework;
using Nop.Web.Framework.Themes;

namespace Nop.Plugin.Misc.MmsAdmin.Infrasctructure
{
    public class MmsAdminViewEngine : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            #region public side

            if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                Debug.WriteLine(context.ViewName);
                if (context.ViewName == "ProductTemplate.MmsVideo")
                {
                    viewLocations = new[] {
                        $"~/Plugins/Misc.MmsAdmin/Themes/"+theme+"/Views/Shared/ProductTemplate.MmsVideo.cshtml",
                        $"~/Plugins/Misc.MmsAdmin/Views/ProductTemplate.MmsVideo.cshtml"
                    }
                    .Concat(viewLocations);
                }

                if (context.ViewName == "_ProductDetailsVideo")
                {
                    viewLocations = new[] {
                        $"~/Plugins/Misc.MmsAdmin/Themes/"+theme+"/Views/Shared/_ProductDetailsVideo.cshtml",
                        $"~/Plugins/Misc.MmsAdmin/Views/_ProductDetailsVideo.cshtml"
                    }
                    .Concat(viewLocations);
                }

                if (context.ViewName == "DownloadableProducts")
                {
                    viewLocations = new[] {
                        $"~/Plugins/Misc.MmsAdmin/Themes/"+theme+"/Views/Shared/DownloadableProducts.cshtml",
                        $"~/Plugins/Misc.MmsAdmin/Views/DownloadableProducts.cshtml"
                    }
                    .Concat(viewLocations);
                }


            }

            #endregion

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            if (context.AreaName?.Equals(AreaNames.Admin) ?? false)
                return;

            var themeContext = (IThemeContext)context.ActionContext.HttpContext.RequestServices.GetService(typeof(IThemeContext));
            context.Values[THEME_KEY] = themeContext.WorkingThemeName;
        }

    }
}