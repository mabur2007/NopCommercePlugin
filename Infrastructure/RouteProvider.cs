using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Tax.Avalara.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {

            //override some of default routes in Admin area
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.MmsAdmin.GetDownload", "Plugins/MmsAdmin/GetDownload",
                new { controller = "MmsAdminDownload", action = "GetDownload"});

            //override some of default routes in Admin area
            endpointRouteBuilder.MapControllerRoute("GetDownload", "Plugins/MmsAdmin/GetDownload",
                new { controller = "MmsAdminDownload", action = "GetDownload" });

            //override some of default routes in Admin area
            endpointRouteBuilder.MapControllerRoute("Plugin.Misc.MmsAdmin.GetDownloadMms", "Plugins/MmsAdmin/GetDownloadMms",
                new { controller = "MmsAdminDownload", action = "GetDownloadMms" });

            //override some of default routes in Admin area
            endpointRouteBuilder.MapControllerRoute("GetDownloadMms", "Plugins/MmsAdmin/GetDownloadMms",
                new { controller = "MmsAdminDownload", action = "GetDownloadMms" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 1000; //set a value that is greater than the default one in Nop.Web to override routes
    }
}