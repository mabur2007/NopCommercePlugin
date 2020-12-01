using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.MmsAdmin.Models;
using Nop.Plugin.Misc.MmsAdmin.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Controllers;

namespace Nop.Plugin.Misc.MmsAdmin.Controllers
{
    public class MmsAdminDownloadController : DownloadController
    {
        private readonly CustomerSettings _customerSettings;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly MmsAdminService _mmsAdminService;
        private readonly ICustomerService _customerService;

        public MmsAdminDownloadController(CustomerSettings customerSettings,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IProductService productService,
            IWorkContext workContext,
            MmsAdminService mmsAdminService,
            ICustomerService customerService) : base(
                 customerSettings,
            downloadService,
            localizationService,
            orderService,
            productService,
            workContext)

        {
            _customerSettings = customerSettings;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _orderService = orderService;
            _productService = productService;
            _workContext = workContext;
            _mmsAdminService = mmsAdminService;
            _customerService = customerService;
        }

        public override IActionResult GetDownload(Guid orderItemId, bool agree = false)
        {
            Debug.WriteLine("**************In Override GetDownload");
            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            //if (orderItem == null)
            //    return InvokeHttp404();

            var order = _orderService.GetOrderById(orderItem.OrderId);

            if (!_orderService.IsDownloadAllowed(orderItem))
                return Content("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return Challenge();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                    return Content("This is not your order");
            }

            var product = _productService.GetProductById(orderItem.ProductId);

            var download = _downloadService.GetDownloadById(product.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (product.HasUserAgreement && !agree)
                return RedirectToRoute("DownloadUserAgreement", new { orderItemId = orderItemId });


            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return Content(string.Format(_localizationService.GetResource("DownloadableProducts.ReachedMaximumNumber"), product.MaxNumberOfDownloads));

            if (download.UseDownloadUrl)
            {
                //increase download
                orderItem.DownloadCount++;
                _orderService.UpdateOrderItem(orderItem);

                //return result
                //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
                //In this case, it is not relevant. Url may not be local.
                return new RedirectResult(download.DownloadUrl);
            }

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //increase download
            orderItem.DownloadCount++;
            _orderService.UpdateOrderItem(orderItem);

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }

        public IActionResult GetDownloadMms(Guid orderItemId, int part, bool agree = false )
        {
            Debug.WriteLine("**************In Override GetDownload MMS***** " + part.ToString());

            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            //if (orderItem == null)
            //    return InvokeHttp404();

            var order = _orderService.GetOrderById(orderItem.OrderId);

            if (!_orderService.IsDownloadAllowed(orderItem))
                return Content("Downloads are not allowed");

            if (_customerSettings.DownloadableProductsValidateUser)
            {
                if (_workContext.CurrentCustomer == null)
                    return Challenge();

                if (order.CustomerId != _workContext.CurrentCustomer.Id)
                    return Content("This is not your order");
            }

            var product = _productService.GetProductById(orderItem.ProductId);

            var download = _downloadService.GetDownloadById(product.DownloadId);
            if (download == null)
                return Content("Download is not available any more.");

            if (product.HasUserAgreement && !agree)
                return RedirectToRoute("DownloadUserAgreement", new { orderItemId = orderItemId });


            if (!product.UnlimitedDownloads && orderItem.DownloadCount >= product.MaxNumberOfDownloads)
                return Content(string.Format(_localizationService.GetResource("DownloadableProducts.ReachedMaximumNumber"), product.MaxNumberOfDownloads));

            if (download.UseDownloadUrl)
            {
                //WGR
                var cust = _customerService.GetCustomerById(order.CustomerId);
                var mms_prod_id = _mmsAdminService.MmsGetMmsItemId(product.Id);
                //If MMS download item
                if (mms_prod_id > 0)
                {
                    string callback_url = null;
                    MmsJsonGetDownloadLinkv2 download_link = new MmsJsonGetDownloadLinkv2();


                    var mmsadmin_return = _mmsAdminService.MmsJsonGetDownloadLinkv2(cust.EmailToRevalidate, mms_prod_id, part, callback_url, out download_link);
                    if (mmsadmin_return == "success")
                    {
                        if (download_link.Status == "ready")
                        {
                            var download_url = download_link.DownloadLink;
                            return new RedirectResult(download_url);
                        }
                        else
                        {
                            Debug.WriteLine("Not ready in GetDownloadMms" + download_link.Status);
                            return Content(download_link.Status);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error in GetDownloadMms" + mmsadmin_return);
                        return Content("Download data is not available just now.");
                    }
                }
                else // Just normal download
                {
                    orderItem.DownloadCount++;
                    _orderService.UpdateOrderItem(orderItem);

                    //return result
                    //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
                    //In this case, it is not relevant. Url may not be local.
                    return new RedirectResult(download.DownloadUrl);
                }
            }

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //increase download
            orderItem.DownloadCount++;
            _orderService.UpdateOrderItem(orderItem);

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : product.Id.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }
    }
}
