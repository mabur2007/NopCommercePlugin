using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Common;
using Nop.Plugin.Misc.MmsAdmin.Domain;
using Nop.Plugin.Misc.MmsAdmin.Models;
using Nop.Plugin.Misc.MmsAdmin.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Misc.MmsDataImport;
using V4;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using Nop.Plugin.Misc.MmsDataImport;

namespace Nop.Plugin.Misc.MmsAdmin.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class MmsAdminController : BasePluginController
    {


        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly AddressSettings _addressSettings;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly MMSSettings _mmsSettings;
        private readonly MmsDataImportDirect _importManagerOverride;
        private readonly MmsAdminService _mmsAdminService;


        public MmsAdminController(IAddressService addressService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStateProvinceService stateProvinceService,
            AddressSettings customerSettings,
            INotificationService notificationService,
            ISettingService settingService,
                        MMSSettings mmsSettings,
                       MmsDataImportDirect importManagerOverride,
                       MmsAdminService mmsAdminService)

        {
            _addressService = addressService;
            _countryService = countryService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _stateProvinceService = stateProvinceService;
            _addressSettings = customerSettings;
            _notificationService = notificationService;
            _settingService = settingService;
            _mmsSettings = mmsSettings;
            _importManagerOverride = importManagerOverride;
            _mmsAdminService = mmsAdminService;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            var model = new ConfigurationsModel();
            //PrepareModel(model);
            model.DollarExchangeRate = _mmsSettings.DollarExchangeRate;
            model.ShippingMultiplyer = _mmsSettings.ShippingMultiplyer;
            model.MarkupMultiplyerLower = _mmsSettings.MarkupMultiplyerLower;
            model.MarkupMultiplyerUpper = _mmsSettings.MarkupMultiplyerUpper;
            model.MarkupMutiplyerThreshold = _mmsSettings.MarkupMutiplyerThreshold;
            model.LoadFromDate = _mmsSettings.LoadFromDate;
            model.LoadUntilDate = _mmsSettings.LoadUntilDate;
            model.LastLoadedDate = _mmsSettings.LastLoadedDate;
            model.MMSusername = _mmsSettings.MMSusername;
            model.MMSpassword = _mmsSettings.MMSpassword;

            Debug.WriteLine("Before return view");

            return View("~/Plugins/Misc.MmsAdmin/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationsModel model)
        {

            if (!ModelState.IsValid)
                return Configure();

            //save settings

            _mmsSettings.DollarExchangeRate = model.DollarExchangeRate;
            _mmsSettings.ShippingMultiplyer = model.ShippingMultiplyer;
            _mmsSettings.MarkupMultiplyerLower = model.MarkupMultiplyerLower;
            _mmsSettings.MarkupMultiplyerUpper = model.MarkupMultiplyerUpper;
            _mmsSettings.MarkupMutiplyerThreshold = model.MarkupMutiplyerThreshold;
            _mmsSettings.LoadFromDate = model.LoadFromDate;
            _mmsSettings.LoadUntilDate = model.LoadUntilDate;
            _mmsSettings.LastLoadedDate = model.LastLoadedDate;
            _mmsSettings.MMSusername = model.MMSusername;
            _mmsSettings.MMSpassword = model.MMSpassword;

            _settingService.SaveSetting(_mmsSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public ActionResult MmsDataLoad()
        {
            // Read Items
            Debug.WriteLine(">>>>In Mms_admin_conntroller");

            //            string Username = "bill@davenportsmagic.co.uk";
            //            string Password = "mA1389giC";

            string Username = _mmsSettings.MMSusername;
            string Password = _mmsSettings.MMSpassword;
            DateTime load_from_date = _mmsSettings.LoadFromDate;
            DateTime load_until_date = _mmsSettings.LoadUntilDate;

            V4.SoapAuthenticationHeader authentication = new V4.SoapAuthenticationHeader();
            authentication.Username = Username;
            authentication.Password = Password;

            //===========
            GetHelloRequest request = new GetHelloRequest();
            request.SoapAuthenticationHeader = authentication;
            V4.V4SoapClient client = new V4SoapClient(V4SoapClient.EndpointConfiguration.V4Soap12);
            GetHelloResponse response = client.GetHelloAsync(request).Result;
            //==========

            Debug.WriteLine(">>>>>>>>>>>>>>>>>>>>");
            Debug.WriteLine(response.message.ToString());
            Debug.WriteLine(response.GetHelloResult.Success.ToString());
            Debug.WriteLine(response.GetHelloResult.ToString());
            Debug.WriteLine(">>>>>>>>>>>>>>>>>>>>");

            MmsDataRead mmsDataRead = new MmsDataRead();
            List<MmsItemRecord> mmsItemRecordList = new List<MmsItemRecord> { };

            int page = 1;

            mmsItemRecordList = mmsDataRead.DataReadFileBetweenPaged(load_from_date, load_until_date, Username, Password, page); // Read new items
            //var mmsItemRecordListb = mmsItemRecordLista.Result;


            if (mmsItemRecordList != null)
            {
                Debug.WriteLine("440: Controller xx LoadLatestMmsItemsPagedInsert " + mmsItemRecordList.Count.ToString());


                //               MmsDataImportDirect mmsDataImportDirect = new MmsDataImportDirect;
                _importManagerOverride.MmsDataImportInsert2(mmsItemRecordList);
            }
            else
            {
                Debug.WriteLine("Read no items MmsAdmin controller");
            }

            //ImportManagerOverride importManagerOverride = new ImportManagerOverride;
            //           _mmsImportManager.MmsDataImportInsert2(mmsItemRecordList);
            //_importManagerOverride.MmsDataImportInsert2(mmsItemRecordList);

            return View("~/Plugins/Misc.PickupInStore2/Views/MmsConfig.cshtml");

        } // Data_Load

        public ActionResult TestDownload()
        {

             Debug.WriteLine("In TestDownload");

            // Create a new WebClient instance.
            //         WebClient myWebClient = new WebClient();

            // Create a new NameValueCollection instance to hold some custom parameters to be posted to the URL.
            //          NameValueCollection myNameValueCollection = new NameValueCollection();

            // Add necessary parameter/value pairs to the name/value container.
            //          myNameValueCollection.Add("APIKey", "e252d955a8df2c516fbc74cfbc37bc97");
            //          myNameValueCollection.Add("Email", "grantjamescox@hotmail.com");
            //      //myNameValueCollection.Add("Age", age);

            //          var responseArray = myWebClient.UploadValues("https://downloads.murphysmagic.com/api/GetCustomer/", myNameValueCollection);
            //
            //         string rawJson =  Encoding.ASCII.GetString(responseArray);
            //         MmsCustJson authTest = JsonConvert.DeserializeObject<MmsCustJson>(rawJson);

            MmsCustJson mmsCustJson = new MmsCustJson();
            var return_string = _mmsAdminService.MmsJsonGetCustomer("mabur2007@hotmail.co.uk", out mmsCustJson);

            Debug.WriteLine(return_string);
            Debug.WriteLine("Cust ID >>  " + mmsCustJson.EmailAddress);
            Debug.WriteLine("Cust ID >>  " + mmsCustJson.FirstName);
            Debug.WriteLine("Cust ID >>  " + mmsCustJson.LastName);
            Debug.WriteLine("Cust ID >>  " + mmsCustJson.Password);
            Debug.WriteLine("Cust ID >>  " + mmsCustJson.CustomerId.ToString());
            Debug.WriteLine("Cust ID >>  " + mmsCustJson.TimeSignedUp.ToString());
            Debug.WriteLine("Cust ID >>  " + mmsCustJson.Language);

           
            return_string = _mmsAdminService.MmsJsonGetCustomer("xxxxx", out mmsCustJson);

            Debug.WriteLine(return_string);
            //
            var mmsIdList = new List<int>();
            var return_string2 = _mmsAdminService.MmsJsonGetDownloadsForCustomer("mabur2007@hotmail.co.uk", out mmsIdList);
            Debug.WriteLine(return_string2);
            if (return_string2 == "success")
            Debug.WriteLine(mmsIdList[0].ToString());


            return View("~/Plugins/Misc.PickupInStore2/Views/MmsConfig.cshtml");
        }

        public IActionResult GetDownload()
        {
            /*var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            if (orderItem == null)
                return InvokeHttp404();

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
    }*/
            //return new RedirectResult(download.DownloadUrl);
            return new RedirectResult("http://downloads.murphysmagic.com/account/api/mms-downloads-api.pdf");
        }
















    }
}
