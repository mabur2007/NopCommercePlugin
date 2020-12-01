using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.MmsAdmin.Domain;
using Nop.Plugin.Misc.MmsAdmin.Models;
using Nop.Plugin.Misc.MmsAdmin.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
//using Nop.Plugin.Misc.PickupInStore2.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using V4;

namespace Nop.Plugin.Misc.MmsAdmin.Services
{
    public class MmsAdminService : IMmsAdminService
    {
        private readonly IRepository<MmsNopVideo> _videoRepository;
        private readonly ILocalizationService _localizationService;
        private readonly MMSSettings _mmsSettings;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;


        public MmsAdminService(IRepository<MmsNopVideo> videoRepository,
                                ILocalizationService localizationService,
                                MMSSettings mmsSettings,
            IOrderService orderService,
            ICustomerService customerService
                                )
        {
            _videoRepository = videoRepository;
            _localizationService = localizationService;
            _mmsSettings = mmsSettings;
            _orderService = orderService;
            _customerService = customerService;
        }

        // Moved from ProductServive
        public virtual int MmsGetMmsItemId(int nop_product_id)
        {
            var query = from p in _videoRepository.Table
                        where p.ProductId == nop_product_id
                        select p.MmsItemId;

            var mms_item_id = query.ToList().FirstOrDefault();

            return mms_item_id;
        }

        //WGR get nop produc_id given mms item id NEW - get the max value
        public virtual int MmsGetNopProdId(int mms_item_id)
        {
            var query = from p in _videoRepository.Table
                        where p.MmsItemId == mms_item_id
                        select p.ProductId;

            if (query.ToList().Count > 1)
            {
                var mms_product_id = query.ToList().Max();
                return mms_product_id;
            }
            else
            {
                var mms_product_id = query.ToList().FirstOrDefault();
                return mms_product_id;
            }
        }

        public virtual void MmsAddSOItems(int[] mms_item_ids, int[] mms_item_qtys)
        {

            // SOAP Authentication header
            V4.SoapAuthenticationHeader authentication = new V4.SoapAuthenticationHeader();
            authentication.Username = _mmsSettings.MMSusername;
            authentication.Password = _mmsSettings.MMSpassword;

            V4.V4SoapClient client = new V4SoapClient(V4SoapClient.EndpointConfiguration.V4Soap12);

            // Output
            string hello;
            var mms_response = client.GetHello(authentication, out hello);

            Debug.WriteLine(mms_response.ToString());
            Debug.WriteLine(hello);

            // Error check
            if (mms_response.Success == false)
            {
                Debug.WriteLine("MMS comms error MmsAddSOItems");
            }

            // V4.MmsResponse mmsResponse = new V4.MmsResponse();
            mms_response.Success = true;

            string salesOrderNumber;
            decimal salesOrderTotal;

            // submit so items to so
            mms_response = client.SubmitSalesOrderLineItems(authentication, mms_item_ids, mms_item_qtys, out salesOrderNumber, out salesOrderTotal);

            if (mms_response.Success == false)
            {
                Debug.WriteLine("Failed to call SubmitSalesOrderLineItems in MmsAddSOItems");
            }
            else
            {
                Debug.WriteLine("Great success to call SubmitSalesOrderLineItems in MmsAddSOItems qty = " + mms_item_ids.Length.ToString());
            }
        }


        /// <summary>
        /// WGR Gets video by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Video</returns>
        public virtual IList<string> GetVideoByProductId(int productId)
        {
            var query = from videot in _videoRepository.Table
                        where videot.ProductId == productId
                        select videot.MmsVideoUrl;

            var videos = query.ToList();

            return videos;
        }

        /// <summary>
        /// WGR Gets video by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Video</returns>
        public virtual string GetOneVideoByProductId(int productId)
        {
            var query = from videot in _videoRepository.Table
                        where videot.ProductId == productId
                        select videot.MmsVideoUrl;

            var videos = query.ToList();

            var video_url = videos[0];

            return video_url;
        }

        /// <summary>
        /// WGR Gets video by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Video</returns>
        /// 
        public virtual int InsertVideo(int productId, string mms_video_url, int mms_item_id)
        //public int InsertVideo()
        {
            MmsNopVideo video = new MmsNopVideo();

            if (productId == 0)
                return 1;

            video.MmsVideoUrl = mms_video_url;
            video.ProductId = productId;
            video.MmsItemId = mms_item_id;

            _videoRepository.Insert(video);

            return 2;
        }

        /// <summary>
        /// WGR Gets video by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Video</returns>
        /// 
        public virtual int UpdateVideo(MmsNopVideo video)
        {
            _videoRepository.Update(video);
            return 1;
        }

        public virtual string MmsJsonGetDownloadsForCustomer(string mmsEmail, out List<int> mmsVideoIdsJson)
        {
            mmsVideoIdsJson = null;
            string return_val = null;
            WebClient myWebClient = new WebClient();

            // Create a new NameValueCollection instance to hold some custom parameters to be posted to the URL.
            NameValueCollection myNameValueCollection = new NameValueCollection();

            // Add necessary parameter/value pairs to the name/value container.
            myNameValueCollection.Add("APIKey", "e252d955a8df2c516fbc74cfbc37bc97");
            myNameValueCollection.Add("Email", mmsEmail);
            //myNameValueCollection.Add("Age", age);

            // 'The Upload(String,NameValueCollection)' implicitly method sets HTTP POST as the request method.            
            var responseArray = myWebClient.UploadValues("https://downloads.murphysmagic.com/api/GetDownloadsForCustomer/", myNameValueCollection);
            string rawJson = Encoding.ASCII.GetString(responseArray);
            if (rawJson.Substring(2, 5) == "error")
            {
                MmsJsonError authTest = JsonConvert.DeserializeObject<MmsJsonError>(rawJson);
                return_val = authTest.error;
            }
            else
            {
                mmsVideoIdsJson = JsonConvert.DeserializeObject<List<int>>(rawJson);
                return_val = "success";
            }

            return return_val;
        }

        public virtual string MmsJsonGetCustomer(string mmsEmail, out MmsCustJson mmsCustJson)
        {
            mmsCustJson = null;
            string return_val = null;
            WebClient myWebClient = new WebClient();

            // Create a new NameValueCollection instance to hold some custom parameters to be posted to the URL.
            NameValueCollection myNameValueCollection = new NameValueCollection();

            // Add necessary parameter/value pairs to the name/value container.
            myNameValueCollection.Add("APIKey", "e252d955a8df2c516fbc74cfbc37bc97");
            myNameValueCollection.Add("Email", mmsEmail);
            //myNameValueCollection.Add("Age", age);

            // 'The Upload(String,NameValueCollection)' implicitly method sets HTTP POST as the request method.            
            var responseArray = myWebClient.UploadValues("https://downloads.murphysmagic.com/api/GetCustomer/", myNameValueCollection);
            string rawJson = Encoding.ASCII.GetString(responseArray);
            if (rawJson.Substring(2, 5) == "error")
            {
                MmsJsonError authTest = JsonConvert.DeserializeObject<MmsJsonError>(rawJson);
                return_val = authTest.error;
            }
            else
            {
                mmsCustJson = JsonConvert.DeserializeObject<MmsCustJson>(rawJson);
                return_val = "success";
            }

            return return_val;
        }

        public virtual string MmsJsonGetDownload(int downloadId, out MmsJsonGetDownload mmsJsonDownload)
        {
            mmsJsonDownload = null;
            string return_val = null;
            WebClient myWebClient = new WebClient();

            // Create a new NameValueCollection instance to hold some custom parameters to be posted to the URL.
            NameValueCollection myNameValueCollection = new NameValueCollection();

            // Add necessary parameter/value pairs to the name/value container.
            myNameValueCollection.Add("APIKey", "e252d955a8df2c516fbc74cfbc37bc97");
            myNameValueCollection.Add("DownloadID", downloadId.ToString());
            //myNameValueCollection.Add("Age", age);

            // 'The Upload(String,NameValueCollection)' implicitly method sets HTTP POST as the request method.            
            var responseArray = myWebClient.UploadValues("https://downloads.murphysmagic.com/api/GetDownload/", myNameValueCollection);
            string rawJson = Encoding.ASCII.GetString(responseArray);
            if (rawJson.Substring(2, 5) == "error")
            {
                MmsJsonError authTest = JsonConvert.DeserializeObject<MmsJsonError>(rawJson);
                return_val = authTest.error;
            }
            else
            {
                mmsJsonDownload = JsonConvert.DeserializeObject<MmsJsonGetDownload>(rawJson);
                return_val = "success";
            }

            return return_val;
        }

        public virtual string MmsJsonGetDownloadFiles(int downloadId, out List<MmsJsonGetDownloadFiles> mmsJsonDownloadFiles)
        {
            mmsJsonDownloadFiles = null;
            string return_val = null;
            WebClient myWebClient = new WebClient();

            // Create a new NameValueCollection instance to hold some custom parameters to be posted to the URL.
            NameValueCollection myNameValueCollection = new NameValueCollection();

            // Add necessary parameter/value pairs to the name/value container.
            myNameValueCollection.Add("APIKey", "e252d955a8df2c516fbc74cfbc37bc97");
            myNameValueCollection.Add("DownloadID", downloadId.ToString());
            //myNameValueCollection.Add("Age", age);

            // 'The Upload(String,NameValueCollection)' implicitly method sets HTTP POST as the request method.            
            var responseArray = myWebClient.UploadValues("https://downloads.murphysmagic.com/api/GetDownloadFiles/", myNameValueCollection);
            string rawJson = Encoding.ASCII.GetString(responseArray);
            if (rawJson.Substring(2, 5) == "error")
            {
                MmsJsonError authTest = JsonConvert.DeserializeObject<MmsJsonError>(rawJson);
                return_val = authTest.error;
            }
            else
            {
                mmsJsonDownloadFiles = JsonConvert.DeserializeObject<List<MmsJsonGetDownloadFiles>>(rawJson);
                return_val = "success";
            }

            return return_val;
        }

        public virtual string MmsJsonAddOrder(string firstName, string lastName, string eMail, List<int> productIdList)
        {
            var prodIdListString = new List<string>();
            string return_val = null;

            foreach (var pId in productIdList)
            {
                prodIdListString.Add(pId.ToString());
            }
            string prodIdList = string.Join(",", prodIdListString.ToArray());

            WebClient myWebClient = new WebClient();

            NameValueCollection myNameValueCollection = new NameValueCollection();
            myNameValueCollection.Add("APIKey", "e252d955a8df2c516fbc74cfbc37bc97");
            myNameValueCollection.Add("FirstName", firstName);
            myNameValueCollection.Add("LastName", lastName);
            myNameValueCollection.Add("Email", eMail);
            myNameValueCollection.Add("ProductIDs", prodIdList);

            var responseArray = myWebClient.UploadValues("https://downloads.murphysmagic.com/api/AddOrder/", myNameValueCollection);
            string rawJson = Encoding.ASCII.GetString(responseArray);
            if (rawJson.Substring(2, 5) == "error")
            {
                MmsJsonError authTest = JsonConvert.DeserializeObject<MmsJsonError>(rawJson);
                return_val = authTest.error;
            }
            else
            {
                return_val = "success";
            }

            return return_val;
        }

        public virtual string MmsJsonGetDownloadLinkv2(string eMail, int productId, int part, string callbackUrl, out MmsJsonGetDownloadLinkv2 mmsJsonDownloadLinkv2)
        {
            mmsJsonDownloadLinkv2 = null;
            string return_val = null;
            WebClient myWebClient = new WebClient();

            NameValueCollection myNameValueCollection = new NameValueCollection();

            // Add necessary parameter/value pairs to the name/value container.
            myNameValueCollection.Add("APIKey", "e252d955a8df2c516fbc74cfbc37bc97");
            myNameValueCollection.Add("Email", eMail);
            myNameValueCollection.Add("ProductID", productId.ToString());
            myNameValueCollection.Add("Part", part.ToString());
            myNameValueCollection.Add("CallbackURL", null);

            var responseArray = myWebClient.UploadValues("https://downloads.murphysmagic.com/api/GetDownloadLinkv2/", myNameValueCollection);
            string rawJson = Encoding.ASCII.GetString(responseArray);
            if (rawJson.Substring(2, 5) == "error")
            {
                MmsJsonError authTest = JsonConvert.DeserializeObject<MmsJsonError>(rawJson);
                return_val = authTest.error;
            }
            else
            {
                mmsJsonDownloadLinkv2 = JsonConvert.DeserializeObject<MmsJsonGetDownloadLinkv2>(rawJson);
                return_val = "success";
            }

            return return_val;
        }

        public virtual int GetMmsDownloadCount(int productId)
        {
            var mms_product_id = MmsGetMmsItemId(productId);
            int return_result = 0;
            List<MmsJsonGetDownloadFiles> mms_dl_list = new List<MmsJsonGetDownloadFiles>();
            string mms_result = MmsJsonGetDownloadFiles(mms_product_id, out mms_dl_list);
            if (mms_result == "success")
            {
                return_result = mms_dl_list.Count();
            }
            else
            {
                Debug.WriteLine("GetMmsDownloadCount error: " + mms_result);   
            }

            return return_result;
        }

        public virtual string MmsGetDownloadStatus(Guid orderItemId, int part)
        {
            string return_status = "error";
            string callback_url = null;
            var orderItem = _orderService.GetOrderItemByGuid(orderItemId);
            var mms_prod_id = MmsGetMmsItemId(orderItem.ProductId);
            var order = _orderService.GetOrderById(orderItem.OrderId);
            var cust = _customerService.GetCustomerById(order.CustomerId);
            MmsJsonGetDownloadLinkv2 download_link = new MmsJsonGetDownloadLinkv2();


            var mmsadmin_return = MmsJsonGetDownloadLinkv2(cust.EmailToRevalidate, mms_prod_id, part, callback_url, out download_link);
            if (mmsadmin_return == "success")
            {
                return_status = download_link.Status;
            }
            else
            {
                Debug.WriteLine("Error in MmsGetDownloadStatus " + mmsadmin_return);
                return_status = mmsadmin_return;
            }

            return return_status;
        }

    }
}
