using System;
using System.Diagnostics;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.MmsAdmin.Domain;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using V4;


namespace Nop.Plugin.Misc.MmsAdmin.Services
{
    public class MmsProductOverride : ProductService
    {
        protected readonly CatalogSettings _catalogSettings;
        protected readonly CommonSettings _commonSettings;
        protected readonly IAclService _aclService;
        protected readonly ICacheKeyService _cacheKeyService;
        protected readonly ICustomerService _customerService;
        protected readonly INopDataProvider _dataProvider;
        protected readonly IDateRangeService _dateRangeService;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly IRepository<AclRecord> _aclRepository;
        protected readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        protected readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<ProductPicture> _productPictureRepository;
        protected readonly IRepository<ProductReview> _productReviewRepository;
        protected readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly IRepository<RelatedProduct> _relatedProductRepository;
        protected readonly IRepository<Shipment> _shipmentRepository;
        protected readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        protected readonly IRepository<StoreMapping> _storeMappingRepository;
        protected readonly IRepository<TierPrice> _tierPriceRepository;
        protected readonly IRepository<Warehouse> _warehouseRepository;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IStoreService _storeService;
        protected readonly IWorkContext _workContext;
        protected readonly LocalizationSettings _localizationSettings;
        private MMSSettings _mms_adminsettings;
        private readonly IRepository<MmsNopVideo> _videoRepository;
        private readonly MMSSettings _mmsSettings;

        public MmsProductOverride(CatalogSettings catalogSettings,
     CommonSettings commonSettings,
     IAclService aclService,
     ICacheKeyService cacheKeyService,
     ICustomerService customerService,
     INopDataProvider dataProvider,
     IDateRangeService dateRangeService,
     IEventPublisher eventPublisher,
     ILanguageService languageService,
     ILocalizationService localizationService,
     IProductAttributeParser productAttributeParser,
     IProductAttributeService productAttributeService,
     IRepository<AclRecord> aclRepository,
     IRepository<CrossSellProduct> crossSellProductRepository,
     IRepository<DiscountProductMapping> discountProductMappingRepository,
     IRepository<Product> productRepository,
     IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
     IRepository<ProductAttributeMapping> productAttributeMappingRepository,
     IRepository<ProductCategory> productCategoryRepository,
     IRepository<ProductPicture> productPictureRepository,
     IRepository<ProductReview> productReviewRepository,
     IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
     IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
     IRepository<RelatedProduct> relatedProductRepository,
     IRepository<Shipment> shipmentRepository,
     IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
     IRepository<StoreMapping> storeMappingRepository,
     IRepository<TierPrice> tierPriceRepository,
     IRepository<Warehouse> warehouseRepositor,
     IStaticCacheManager staticCacheManager,
     IStoreService storeService,
     IStoreMappingService storeMappingService,
     IWorkContext workContext,
     LocalizationSettings localizationSettings,
                 MMSSettings mms_adminsettings,
           IRepository<MmsNopVideo> videoRepository,
                                MMSSettings mmsSettings) :
                      base(
                catalogSettings,
             commonSettings,
             aclService,
             cacheKeyService,
             customerService,
             dataProvider,
             dateRangeService,
             eventPublisher,
             languageService,
             localizationService,
             productAttributeParser,
             productAttributeService,
             aclRepository,
             crossSellProductRepository,
            discountProductMappingRepository,
            productRepository,
             productAttributeCombinationRepository,
            productAttributeMappingRepository,
             productCategoryRepository,
             productPictureRepository,
             productReviewRepository,
            productReviewHelpfulnessRepository,
             productWarehouseInventoryRepository,
            relatedProductRepository,
            shipmentRepository,
             stockQuantityHistoryRepository,
             storeMappingRepository,
         tierPriceRepository,
            warehouseRepositor,
            staticCacheManager,
           storeService,
             storeMappingService,
          workContext,
             localizationSettings
                )
        {
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _aclService = aclService;
            _cacheKeyService = cacheKeyService;
            _customerService = customerService;
            _dataProvider = dataProvider;
            _dateRangeService = dateRangeService;
            _eventPublisher = eventPublisher;
            _languageService = languageService;
            _localizationService = localizationService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _aclRepository = aclRepository;
            _crossSellProductRepository = crossSellProductRepository;
            _discountProductMappingRepository = discountProductMappingRepository;
            _productRepository = productRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productCategoryRepository = productCategoryRepository;
            _productPictureRepository = productPictureRepository;
            _productReviewRepository = productReviewRepository;
            _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _relatedProductRepository = relatedProductRepository;
            _shipmentRepository = shipmentRepository;
            _stockQuantityHistoryRepository = stockQuantityHistoryRepository;
            _storeMappingRepository = storeMappingRepository;
            _tierPriceRepository = tierPriceRepository;
            _warehouseRepository = warehouseRepositor;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _workContext = workContext;
            _localizationSettings = localizationSettings;
            _mms_adminsettings = mms_adminsettings;
            _videoRepository = videoRepository;
                        _mmsSettings = mmsSettings;
        }

        /// <summary>
        /// WGR Gets video by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Video</returns>
        /// 
        public virtual int GetMmsStockQty(Product product)
        {
            if (product == null)
            {
                return 0;
            }

            V4.SoapAuthenticationHeader authentication = new V4.SoapAuthenticationHeader();
            authentication.Username = _mmsSettings.MMSusername;
            authentication.Password = _mmsSettings.MMSpassword;

            V4.V4SoapClient client = new V4SoapClient(V4SoapClient.EndpointConfiguration.V4Soap12);

            // Output
            string hello;
            var response_hello = client.GetHello(authentication, out hello);

            Debug.WriteLine(response_hello.ToString());
            Debug.WriteLine(hello);

            // Error check
            if (response_hello.Success == false)
            {
                Debug.WriteLine("MMS Comms error GetMmsStockQty");
                return 0;
            }

            var query = from p in _videoRepository.Table
                        where p.ProductId == product.Id
                        select p.MmsItemId;

            var mms_item_id = query.ToList().FirstOrDefault();

            Debug.WriteLine("In GetMmsStockQty mms_item_id, product_id = " + mms_item_id.ToString() + " " + product.Id.ToString());

            // Get stock qty from MMS

            int[] mmsItemKeys = new int[1];
            int[] mmsItemQty = new int[1];


            mmsItemKeys[0] = mms_item_id;
            mmsItemQty[0] = 0;

            // Get first page of results
            var mmsResponse = client.GetInventoryItemsAvailableQuantities(authentication, mmsItemKeys, out mmsItemQty);

            // Error check
            if (mmsResponse.Success == false)
            {
                Debug.WriteLine("MMS comms Error Getting Qty in GetMmsStockQty");
                return 0;
            }

            // Ok, now update the db with new stock qty

            product.StockQuantity = mmsItemQty[0];

            UpdateProduct(product);

            return mmsItemQty[0];
        }

        /// <summary>
        /// Get total quantity
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="useReservedQuantity">
        /// A value indicating whether we should consider "Reserved Quantity" property 
        /// when "multiple warehouses" are used
        /// </param>
        /// <param name="warehouseId">
        /// Warehouse identifier. Used to limit result to certain warehouse.
        /// Used only with "multiple warehouses" enabled.
        /// </param>
        /// <returns>Result</returns>
        public override int GetTotalStockQuantity(Product product, bool useReservedQuantity = true, int warehouseId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            Debug.WriteLine("In Override GetTotalStockQuantity line 200 GetTotalStockQuantity");
            // This is important - each time product viewed the stock qty is updated
            var mmsstockQuantity = GetMmsStockQty(product);
            Debug.WriteLine("Stock Qty:::");
            Debug.WriteLine(mmsstockQuantity.ToString());

            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
            {
                //We can calculate total stock quantity when 'Manage inventory' property is set to 'Track inventory'
                return 0;
            }

            if (!product.UseMultipleWarehouses)
                return mmsstockQuantity;

            var pwi = _productWarehouseInventoryRepository.Table.Where(wi => wi.ProductId == product.Id);

            if (warehouseId > 0)
            {
                pwi = pwi.Where(x => x.WarehouseId == warehouseId);
            }

            var result = pwi.Sum(x => x.StockQuantity);
            if (useReservedQuantity)
            {
                result -= pwi.Sum(x => x.ReservedQuantity);
            }

            return result;
        }



        /// <summary>
        /// Formats the stock availability/quantity message
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Selected product attributes in XML format (if specified)</param>
        /// <returns>The stock message</returns>
        public override string FormatStockMessage(Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            Debug.WriteLine("In Override FormatStockMessage line 236 FormatStockMessage");
            // This is important - each time product viewed the stock qty is updated
            var stockQuantity = GetMmsStockQty(product);
            Debug.WriteLine("Stock Qty:::");
            Debug.WriteLine(stockQuantity.ToString());

            var stockMessage = string.Empty;

            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.ManageStock:
                    stockMessage = GetStockMessage(product, stockMessage);
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    stockMessage = GeStockMessage(product, attributesXml);
                    break;
            }

            return stockMessage;
        }

        public override void AdjustInventory(Product product, int quantityToChange, string attributesXml = "", string message = "")
        {
            Debug.WriteLine("*****In New Override Adjust Inventory*****");

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantityToChange == 0)
                return;

            //WGR Get the MMS total stock 
            var mms_stock_qty = GetMmsStockQty(product);

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                //previous stock
                var prevStockQuantity = GetTotalStockQuantity(product);

                //update stock quantity
                if (product.UseMultipleWarehouses)
                {
                    //use multiple warehouses
                    if (quantityToChange < 0)
                        ReserveInventory(product, quantityToChange);
                    else
                        UnblockReservedInventory(product, quantityToChange);
                }
                else
                {
                    //do not use multiple warehouses
                    //simple inventory management
                    product.StockQuantity += quantityToChange;
                    UpdateProduct(product);

                    //quantity change history
                    AddStockQuantityHistoryEntry(product, quantityToChange, product.StockQuantity, product.WarehouseId, message);
                }

                //qty is reduced. check if minimum stock quantity is reached
                if ((quantityToChange < 0) && (product.MinStockQuantity >= mms_stock_qty))  //WGR
                {
                    //what should we do now? disable buy button, unpublish the product, or do nothing? check "Low stock activity" property
                    switch (product.LowStockActivity)
                    {
                        case LowStockActivity.DisableBuyButton:
                            product.DisableBuyButton = true;
                            product.DisableWishlistButton = true;
                            UpdateProduct(product);
                            break;
                        case LowStockActivity.Unpublish:
                            product.Published = false;
                            UpdateProduct(product);
                            break;
                        default:
                            break;
                    }
                }
                //qty is increased. product is back in stock (minimum stock quantity is reached again)?
                if (_catalogSettings.PublishBackProductWhenCancellingOrders)
                {
                    if (quantityToChange > 0 && prevStockQuantity <= product.MinStockQuantity && product.MinStockQuantity < mms_stock_qty)
                    {
                        switch (product.LowStockActivity)
                        {
                            case LowStockActivity.DisableBuyButton:
                                product.DisableBuyButton = false;
                                product.DisableWishlistButton = false;
                                UpdateProduct(product);
                                break;
                            case LowStockActivity.Unpublish:
                                product.Published = true;
                                UpdateProduct(product);
                                break;
                            default:
                                break;
                        }
                    }
                }

                //send email notification
                if ((quantityToChange < 0) && (mms_stock_qty < product.NotifyAdminForQuantityBelow)) //WGR
                {
                    var workflowMessageService = EngineContext.Current.Resolve<IWorkflowMessageService>();
                    workflowMessageService.SendQuantityBelowStoreOwnerNotification(product, _localizationSettings.DefaultAdminLanguageId);
                }
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);
                if (combination != null)
                {
                    combination.StockQuantity += quantityToChange;
                    _productAttributeService.UpdateProductAttributeCombination(combination);

                    //quantity change history
                    AddStockQuantityHistoryEntry(product, quantityToChange, combination.StockQuantity, message: message, combinationId: combination.Id);

                    //send email notification
                    if (quantityToChange < 0 && combination.StockQuantity < combination.NotifyAdminForQuantityBelow)
                    {
                        var workflowMessageService = EngineContext.Current.Resolve<IWorkflowMessageService>();
                        workflowMessageService.SendQuantityBelowStoreOwnerNotification(combination, _localizationSettings.DefaultAdminLanguageId);
                    }
                }

            }
        }// AdjustInventory



    } //MmsProductOverride class 
} // NameSpece
