using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.MmsAdmin.Services
{
    public class MmsOrderProcessingOverride : OrderProcessingService
    {
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressService _addressService;
        private readonly IAffiliateService _affiliateService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxService _taxService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly MmsAdminService _mmsadminService;

        public MmsOrderProcessingOverride(
         CurrencySettings currencySettings,
         IAddressService addressService,
         IAffiliateService affiliateService,
         ICheckoutAttributeFormatter checkoutAttributeFormatter,
         ICountryService countryService,
         ICurrencyService currencyService,
         ICustomerActivityService customerActivityService,
         ICustomerService customerService,
         ICustomNumberFormatter customNumberFormatter,
         IDiscountService discountService,
         IEncryptionService encryptionService,
         IEventPublisher eventPublisher,
         IGenericAttributeService genericAttributeService,
         IGiftCardService giftCardService,
         ILanguageService languageService,
         ILocalizationService localizationService,
         ILogger logger,
         IOrderService orderService,
         IOrderTotalCalculationService orderTotalCalculationService,
         IPaymentPluginManager paymentPluginManager,
         IPaymentService paymentService,
         IPdfService pdfService,
         IPriceCalculationService priceCalculationService,
         IPriceFormatter priceFormatter,
         IProductAttributeFormatter productAttributeFormatter,
         IProductAttributeParser productAttributeParser,
         IProductService productService,
         IRewardPointService rewardPointService,
         IShipmentService shipmentService,
         IShippingService shippingService,
         IShoppingCartService shoppingCartService,
         IStateProvinceService stateProvinceService,
         ITaxService taxService,
         IVendorService vendorService,
         IWebHelper webHelper,
         IWorkContext workContext,
         IWorkflowMessageService workflowMessageService,
         LocalizationSettings localizationSettings,
         OrderSettings orderSettings,
         PaymentSettings paymentSettings,
         RewardPointsSettings rewardPointsSettings,
         ShippingSettings shippingSettings,
         TaxSettings taxSettings,
         MmsAdminService mmsadminService) : base(
         currencySettings,
         addressService,
         affiliateService,
         checkoutAttributeFormatter,
         countryService,
         currencyService,
         customerActivityService,
         customerService,
         customNumberFormatter,
         discountService,
         encryptionService,
         eventPublisher,
         genericAttributeService,
         giftCardService,
         languageService,
         localizationService,
         logger,
         orderService,
          orderTotalCalculationService,
         paymentPluginManager,
          paymentService,
         pdfService,
         priceCalculationService,
         priceFormatter,
         productAttributeFormatter,
         productAttributeParser,
          productService,
          rewardPointService,
         shipmentService,
         shippingService,
         shoppingCartService,
          stateProvinceService,
         taxService,
         vendorService,
          webHelper,
         workContext,
         workflowMessageService,
          localizationSettings,
         orderSettings,
        paymentSettings,
         rewardPointsSettings,
         shippingSettings,
         taxSettings)
        {
            _currencySettings = currencySettings;
            _addressService = addressService;
            _affiliateService = affiliateService;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _customNumberFormatter = customNumberFormatter;
            _discountService = discountService;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _languageService = languageService;
            _localizationService = localizationService;
            _logger = logger;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _taxService = taxService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
            _mmsadminService = mmsadminService;
        }

        /// <summary>
        /// Move shopping cart items to order items
        /// </summary>
        /// <param name="details">Place order container</param>
        /// <param name="order">Order</param>
        protected override void MoveShoppingCartItemsToOrderItems(PlaceOrderContainer details, Order order)
        {
            //wgr
            var cart_size = details.Cart.Count();
            int[] mms_item_ids = new int[cart_size];
            int[] mms_item_qtys = new int[cart_size];
            int mms_i = 0;
            var mmsDownloadList = new List<int>();
            var firstName = details.BillingAddress.FirstName;
            var lastName = details.BillingAddress.LastName;
            var eMail = details.Customer.Email;

            //wgr
            Debug.WriteLine(" ******  In Override for MoveShoppingCartItemsToOrderItems");

            foreach (var sc in details.Cart)
            {
                var product = _productService.GetProductById(sc.ProductId);

                //WGR build mms items for sales_order
                if (product.IsDownload == false)
                {
                    var mms_id = _mmsadminService.MmsGetMmsItemId(sc.ProductId);
                    if (mms_id > 0)
                    {
                        mms_item_ids[mms_i] = mms_id;
                        mms_item_qtys[mms_i] = sc.Quantity;
                        mms_i++;
                    }
                }
                else
                {
                    var mms_id = _mmsadminService.MmsGetMmsItemId(sc.ProductId);
                    if (mms_id > 0)
                    {
                        mmsDownloadList.Add(mms_id);
                    }
                }
                //WGR

                //prices
                var scUnitPrice = _shoppingCartService.GetUnitPrice(sc);
                var scSubTotal = _shoppingCartService.GetSubTotal(sc, true, out var discountAmount,
                    out var scDiscounts, out _);
                var scUnitPriceInclTax =
                    _taxService.GetProductPrice(product, scUnitPrice, true, details.Customer, out var _);
                var scUnitPriceExclTax =
                    _taxService.GetProductPrice(product, scUnitPrice, false, details.Customer, out _);
                var scSubTotalInclTax =
                    _taxService.GetProductPrice(product, scSubTotal, true, details.Customer, out _);
                var scSubTotalExclTax =
                    _taxService.GetProductPrice(product, scSubTotal, false, details.Customer, out _);
                var discountAmountInclTax =
                    _taxService.GetProductPrice(product, discountAmount, true, details.Customer, out _);
                var discountAmountExclTax =
                    _taxService.GetProductPrice(product, discountAmount, false, details.Customer, out _);
                foreach (var disc in scDiscounts)
                    if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                        details.AppliedDiscounts.Add(disc);

                //attributes
                var attributeDescription =
                    _productAttributeFormatter.FormatAttributes(product, sc.AttributesXml, details.Customer);

                var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);

                //save order item
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    UnitPriceInclTax = scUnitPriceInclTax,
                    UnitPriceExclTax = scUnitPriceExclTax,
                    PriceInclTax = scSubTotalInclTax,
                    PriceExclTax = scSubTotalExclTax,
                    OriginalProductCost = _priceCalculationService.GetProductCost(product, sc.AttributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = sc.AttributesXml,
                    Quantity = sc.Quantity,
                    DiscountAmountInclTax = discountAmountInclTax,
                    DiscountAmountExclTax = discountAmountExclTax,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    ItemWeight = itemWeight,
                    RentalStartDateUtc = sc.RentalStartDateUtc,
                    RentalEndDateUtc = sc.RentalEndDateUtc
                };

                _orderService.InsertOrderItem(orderItem);

                //gift cards
                AddGiftCards(product, sc.AttributesXml, sc.Quantity, orderItem, scUnitPriceExclTax);

                //inventory
                _productService.AdjustInventory(product, -sc.Quantity, sc.AttributesXml,
                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.PlaceOrder"), order.Id));
            }
            //WGR Add to MMS SO
            _mmsadminService.MmsAddSOItems(mms_item_ids, mms_item_qtys);
            //WGR Add downloads
            var mmsSuccess = _mmsadminService.MmsJsonAddOrder(firstName, lastName, eMail, mmsDownloadList);
            if (mmsSuccess != "success")
            {
                Debug.WriteLine("Error Adding Downloads: " + mmsSuccess);
            }
            //clear shopping cart
            details.Cart.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci, false));
        }

    }
}
