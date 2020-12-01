using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.MmsAdmin.Domain;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using OfficeOpenXml;
using V4;

namespace Nop.Plugin.Misc.MmsAdmin.Services
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class ImportManagerOverride : ImportManager
    {

        //it's quite fast hash (to cheaply distinguish between objects)
        private const string IMAGE_HASH_ALGORITHM = "SHA512";

        private const string UPLOADS_TEMP_PATH = "~/App_Data/TempUploads";


        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICountryService _countryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INopDataProvider _dataProvider;
        private readonly IDateRangeService _dateRangeService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IManufacturerService _manufacturerService;
        private readonly IMeasureService _measureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INopFileProvider _fileProvider;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IShippingService _shippingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;
        private MMSSettings _mms_adminsettings;
        private readonly MmsAdminService _mmsAdminService;



        public ImportManagerOverride(CatalogSettings catalogSettings,
            ICategoryService categoryService,
            ICountryService countryService,
            ICustomerActivityService customerActivityService,
            INopDataProvider dataProvider,
            IDateRangeService dateRangeService,
            IHttpClientFactory httpClientFactory,
            ILocalizationService localizationService,
            ILogger logger,
            IManufacturerService manufacturerService,
            IMeasureService measureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INopFileProvider fileProvider,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IServiceScopeFactory serviceScopeFactory,
            IShippingService shippingService,
            ISpecificationAttributeService specificationAttributeService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ITaxCategoryService taxCategoryService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings,
                            MMSSettings mms_adminsettings,
                                            MmsAdminService mmsAdminService) :
                      base(
                         catalogSettings,
            categoryService,
             countryService,
             customerActivityService,
             dataProvider,
             dateRangeService,
             httpClientFactory,
             localizationService,
            logger,
             manufacturerService,
             measureService,
             newsLetterSubscriptionService,
            fileProvider,
            pictureService,
             productAttributeService,
           productService,
             productTagService,
             productTemplateService,
             serviceScopeFactory,
             shippingService,
            specificationAttributeService,
             stateProvinceService,
             storeContext,
             storeMappingService,
             storeService,
             taxCategoryService,
            urlRecordService,
             vendorService,
            workContext,
            mediaSettings,
            vendorSettings)

        {
            _catalogSettings = catalogSettings;
            _categoryService = categoryService;
            _countryService = countryService;
            _customerActivityService = customerActivityService;
            _dataProvider = dataProvider;
            _dateRangeService = dateRangeService;
            _httpClientFactory = httpClientFactory;
            _fileProvider = fileProvider;
            _localizationService = localizationService;
            _logger = logger;
            _manufacturerService = manufacturerService;
            _measureService = measureService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _productTagService = productTagService;
            _productTemplateService = productTemplateService;
            _serviceScopeFactory = serviceScopeFactory;
            _shippingService = shippingService;
            _specificationAttributeService = specificationAttributeService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _taxCategoryService = taxCategoryService;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
            _mms_adminsettings = mms_adminsettings;
            _mmsAdminService = mmsAdminService;
        }

        public struct NopCatMap
        {
            public string CatName { get; set; }
            public int ParentCatId { get; set; }
        }

        protected class PictureMetadata
        {
            public string PictureName { get; set; }

            public string PicturePath { get; set; }
        }

        public NopCatMap mmsCategoryMap(string mms_cat)
        {
            NopCatMap nop_cat = new NopCatMap();

            switch (mms_cat)
            {
                case "Card Magic and Trick Decks":
                    nop_cat.CatName = "Card Magic";
                    nop_cat.ParentCatId = 0; // No Parent
                    break;

                case "Close-Up Magic":
                    nop_cat.CatName = "Close Up";
                    nop_cat.ParentCatId = 0; // No Parent
                    break;

                case "Close Up Performer":
                    nop_cat.CatName = "Close Up";
                    nop_cat.ParentCatId = 0; // No Parent
                    break;

                case "Mentalism,Bizarre and Psychokinesis Perf":
                    nop_cat.CatName = "Mentalism";
                    nop_cat.ParentCatId = 0; // No Parent
                    break;

                case "Theory, History and Business":
                    nop_cat.CatName = "Theory, History and Business";
                    nop_cat.ParentCatId = 27; // General Magic
                    break;

                case "Stage / Parlor Performer":
                    nop_cat.CatName = "Stage Magic";
                    nop_cat.ParentCatId = 27; // General Magic
                    break;

                case "Exclusive":
                    nop_cat.CatName = "Special Magic";
                    nop_cat.ParentCatId = 27; // General Magic
                    break;

                case "Mentalism":
                    nop_cat.CatName = "Mentalism";
                    nop_cat.ParentCatId = 0; // No Parent
                    break;

                case "Comedy Performer":
                    nop_cat.CatName = "Comedy Magic";
                    nop_cat.ParentCatId = 27; // General Magic
                    break;

                case "Street Performer":
                    nop_cat.CatName = "Street Magic";
                    nop_cat.ParentCatId = 27; // General Magic
                    break;

                case "Walk Around Performer":
                    nop_cat.CatName = "Close Up";
                    nop_cat.ParentCatId = 0; // No Parent
                    break;

                case "Refills":
                    nop_cat.CatName = "Refills";
                    nop_cat.ParentCatId = 28; // Accessories
                    break;

                case "Toy Magic (Toy, Kits, Puzzles)":
                    nop_cat.CatName = "Close Up";
                    nop_cat.ParentCatId = 0; // No Parent
                    break;

                case "Close-Up":
                    nop_cat.CatName = "Close Up";
                    nop_cat.ParentCatId = 0; // No Parent
                    break;

                case "Money Magic":
                    nop_cat.CatName = "Money Magic";
                    nop_cat.ParentCatId = 31; // Close Up
                    break;

                case "Recommended":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 31; // Close Up
                    break;

                case "Sponge and Sponge Magic":
                    nop_cat.CatName = "Sponge Magic";
                    nop_cat.ParentCatId = 31; // Close Up
                    break;

                case "Magic For Kids":
                    nop_cat.CatName = "Kids Magic";
                    nop_cat.ParentCatId = 27; // General
                    break;

                case "Kids Show and Balloon Performer":
                    nop_cat.CatName = "Kids Magic";
                    nop_cat.ParentCatId = 27; // General
                    break;

                case "Gambling Magic":
                    nop_cat.CatName = "Gambling";
                    nop_cat.ParentCatId = 27; // General
                    break;

                case "Gambling Performer":
                    nop_cat.CatName = "Gambling";
                    nop_cat.ParentCatId = 27; // General
                    break;

                case "Magic Staples / Cons":
                    nop_cat.CatName = "Magic Staples";
                    nop_cat.ParentCatId = 28; // Accessories
                    break;

                case "Halloween Themed":
                    nop_cat.CatName = "Halloween Magic";
                    nop_cat.ParentCatId = 27; // General
                    break;

                case "Silk and Silk Magic":
                    nop_cat.CatName = "Silk Magic";
                    nop_cat.ParentCatId = 27; // General
                    break;

                case "Decks (Custom, Standard)":
                    nop_cat.CatName = "Decks";
                    nop_cat.ParentCatId = 0;
                    break;



                case "Special Effects (Fire, Smoke, Sound)":
                    nop_cat.CatName = "Special Effects (Fire, Smoke, Sound)";
                    nop_cat.ParentCatId = 27; // general
                    break;

                case "Tables and Cases":
                    nop_cat.CatName = "Tables and Cases";
                    nop_cat.ParentCatId = 27; // general
                    break;

                case "Posters, Gifts and Collectables":
                    nop_cat.CatName = "Posters, Gifts and Collectables";
                    nop_cat.ParentCatId = 27; // general
                    break;

                case "Black Label":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 0; // no parent
                    break;

                case "Christmas Themed":
                    nop_cat.CatName = "Christmas Themed";
                    nop_cat.ParentCatId = 27; // general
                    break;

                case "Beginners / Kids Mag":
                    nop_cat.CatName = "Beginners";
                    nop_cat.ParentCatId = 0; // no parent
                    break;

                case "Easy To Demo (Shop)":
                    nop_cat.CatName = "Beginners";
                    nop_cat.ParentCatId = 0; // no parent
                    break;

                case "Collectible/Historic":
                    nop_cat.CatName = "Theory, History and Business";
                    nop_cat.ParentCatId = 27; // general
                    break;

                case "Broadcast Special":
                    nop_cat.CatName = "Street Magic";
                    nop_cat.ParentCatId = 27; // general
                    break;

                case "Seasonal - Halloween":
                    nop_cat.CatName = "Halloween Magic";
                    nop_cat.ParentCatId = 27; // general
                    break;

                case "Card Deck":
                    nop_cat.CatName = "Decks";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Available in French":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Available in German":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Available in Korean":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Available in Spanish":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Available in Japan":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Available in Chinese":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Available in Italian":
                    nop_cat.CatName = "Favourites";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Seasonal - Christmas":
                    nop_cat.CatName = "Christmas Themed";
                    nop_cat.ParentCatId = 27; // general
                    break;

                case "Lectures and Conventions":
                    nop_cat.CatName = "Lectures";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Fire Magic":
                    nop_cat.CatName = "Special Effects (Fire, Smoke, Sound)";
                    nop_cat.ParentCatId = 27; // 
                    break;

                case "Illusionist":
                    nop_cat.CatName = "Illusions";
                    nop_cat.ParentCatId = 27; // 
                    break;

                case "Juggling Performer":
                    nop_cat.CatName = "Juggling";
                    nop_cat.ParentCatId = 27; // 
                    break;

                case "Religious and Gospel Performer":
                    nop_cat.CatName = "Gospel Magic";
                    nop_cat.ParentCatId = 27; // 
                    break;


                case "Limited Edition":
                    nop_cat.CatName = "Limited Edition";
                    nop_cat.ParentCatId = 27; // 
                    break;


                case "Utility":
                    nop_cat.CatName = "Utility";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Magazines":
                    nop_cat.CatName = "Magazines";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Magic Set":
                    nop_cat.CatName = "Beginners";
                    nop_cat.ParentCatId = 0; // 
                    break;

                case "Escape Performer":
                    nop_cat.CatName = "Escapology";
                    nop_cat.ParentCatId = 27; // 
                    break;

                default:
                    nop_cat.CatName = mms_cat;
                    nop_cat.ParentCatId = 29; // Non-Categorised general
                    Debug.WriteLine(">>>>>>>>>>>>  In mmsCategoryMap 1213 default >>>" + mms_cat);
                    break;

            }

            return nop_cat;
        }

        public virtual decimal MmSPriceCalc(decimal wholesalePrice, bool isSaleItem)
        {
            decimal postage_rate = 1.1m;
            decimal multiplyer = 2.1m;
            decimal erate = 1.25m;

            var threshold = _mms_adminsettings.MarkupMutiplyerThreshold;

            if (isSaleItem)
            {
                //multiplyer = _mms_adminsettings.MmsSpecialOfferMultiplyer;
            }
            else
            {
                if (wholesalePrice < threshold)
                {
                    multiplyer = _mms_adminsettings.MarkupMultiplyerLower;
                }
                else
                {
                    multiplyer = _mms_adminsettings.MarkupMultiplyerUpper;
                }
            }

            erate = _mms_adminsettings.DollarExchangeRate;

            var retail_price = Decimal.Divide(wholesalePrice, erate) * postage_rate * multiplyer;

            return retail_price;
        }

        //       protected virtual void MmsImportProductImagesUsingServices(IList<ProductPictureMetadata> productPictureMetadataOriginal)
        protected virtual void MmsImportProductImagesUsingServices(Product product, IList<PictureMetadata> picture_original_list, int display_order, bool isNew)
        {
            List<string> picture_new_list = new List<string>();

            foreach (var pp_item in picture_original_list)
            {
                if ((pp_item != null) && (pp_item.PictureName != null))
                {
                    if (pp_item.PictureName.Length > 4) // Need to check for null first
                    {
                        var mmsFullImageWeb = pp_item.PicturePath;
                        var mmsFullImageName = "c:\\bin\\temp\\" + pp_item.PictureName;
                        Debug.WriteLine("In MmsImportManager2, MmsImportProductImagesUsingServices >>>>");
                        Debug.WriteLine(product.Name);
                        Debug.WriteLine(mmsFullImageName);
                        Debug.WriteLine(mmsFullImageWeb);
                        // Use this for release           var mmsFullImageName = "h:\\root\\home\\davmagic-001\\www\\site1\\content\\" + mmsDataItem.ImageFileName;
                        WebClient Client = new WebClient();
                        Client.DownloadFile(mmsFullImageWeb, @mmsFullImageName);
                        picture_new_list.Add(mmsFullImageName);
                    }
                }
            }

            foreach (var picturePath in picture_new_list)
            {
                if ((string.IsNullOrEmpty(picturePath)) || (picturePath.Length < 4))
                {
                    Debug.WriteLine("Picture Path Empty in import using services 201");
                    continue;
                }

                var mimeType = MmsGetMimeTypeFromFilePath(picturePath);
                var newPictureBinary = _fileProvider.ReadAllBytes(picturePath);
                var pictureAlreadyExists = false;
                if (!isNew)
                {
                    //compare with existing product pictures
                    var existingPictures = _pictureService.GetPicturesByProductId(product.Id);
                    foreach (var existingPicture in existingPictures)
                    {
                        var existingBinary = _pictureService.LoadPictureBinary(existingPicture);
                        //picture binary after validation (like in database)
                        var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeType);
                        if (!existingBinary.SequenceEqual(validatedPictureBinary) &&
                            !existingBinary.SequenceEqual(newPictureBinary))
                            continue;
                        //the same picture content
                        pictureAlreadyExists = true;
                        break;
                    }
                }

                if (pictureAlreadyExists)
                    continue;

                try
                {
                    var newPicture = _pictureService.InsertPicture(newPictureBinary, mimeType, _pictureService.GetPictureSeName(product.Name));
                    _productService.InsertProductPicture(new ProductPicture
                    {
                        //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                        //pictures are duplicated
                        //maybe because entity size is too large
                        PictureId = newPicture.Id,
                        DisplayOrder = display_order,
                        ProductId = product.Id
                    });
                    _productService.UpdateProduct(product);
                }
                catch (Exception ex)
                {
                    MmsLogPictureInsertError(picturePath, ex);
                }
            }
        }

        protected virtual Picture MmsLoadPicture(string imageFileName, string name, int? picId = null)
        {

             //_urlRecordService.SaveSlug(productToImport, productToImport.ValidateSeName(seName, productToImport.Name, true), 0);

            string mmsFullImageWeb = null;
            if (imageFileName.Substring(0, 8) == "https://")
            {
                mmsFullImageWeb = imageFileName;
            }
            else
            {
                mmsFullImageWeb = "https://www.murphysmagicsupplies.com/images/" + imageFileName;
            }

            var mmsFullImageName = "c:\\bin\\temp\\" + imageFileName;
            // Use this for release           var mmsFullImageName = "h:\\root\\home\\davmagic-001\\www\\site1\\content\\" + mmsDataItem.ImageFileName;

            WebClient Client = new WebClient();

            Client.DownloadFile(mmsFullImageWeb, @mmsFullImageName);
            Debug.WriteLine("In MmsLoadPicture {0}  {1} >>>", mmsFullImageWeb, mmsFullImageName);


            if (string.IsNullOrEmpty(imageFileName) || !_fileProvider.FileExists(mmsFullImageName))
                return null;

            var mimeTypeP = MmsGetMimeTypeFromFilePath(mmsFullImageName);
            var newPictureBinary = _fileProvider.ReadAllBytes(mmsFullImageName);
            var pictureAlreadyExists = false;
            if (picId != null)
            {
                //compare with existing product pictures
                var existingPicture = _pictureService.GetPictureById(picId.Value);
                if (existingPicture != null)
                {
                    var existingBinary = _pictureService.LoadPictureBinary(existingPicture);
                    //picture binary after validation (like in database)
                    var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeTypeP);
                    if (existingBinary.SequenceEqual(validatedPictureBinary) ||
                        existingBinary.SequenceEqual(newPictureBinary))
                    {
                        pictureAlreadyExists = true;
                    }
                }
            }

            if (pictureAlreadyExists)
                return null;

            var newPicture = _pictureService.InsertPicture(newPictureBinary, mimeTypeP, _pictureService.GetPictureSeName(name));
            return newPicture;
        }

        // For reading flat file download format
        public virtual List<MmsItemRecord> DataReadDownloadFlatfile(Stream stream)
        {
            //          using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            List<MmsItemRecord> mmsItemReturnList = new List<MmsItemRecord>();

            //create a list to hold all the values
            List<string> excelData = new List<string>();
            bool format_error = false;

            //Firstly check that first row has valid MMS column names, if not throw an excetption
            for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
            {
                var col_text = worksheet.Cells[1, col].Value.ToString();
                Debug.WriteLine("Col Text:  " + col_text + " Col:  " + col.ToString());
                switch (col)
                {
                    case 1:
                        if (col_text != "ProductCode") format_error = true;
                        break;
                    case 2:
                        if (col_text != "InternalId") format_error = true;
                        break;
                    case 3:
                        if (col_text != "ArtistOrMagician") format_error = true;
                        break;
                    case 4:
                        if (col_text != "HTMLDescription") format_error = true;
                        break;
                    case 5:
                        if (col_text != "NoHTMLDescription") format_error = true;
                        break;
                    case 6:
                        if (col_text != "AlternateImages") format_error = true;
                        break;
                    case 7:
                        if (col_text != "DateAdded") format_error = true;
                        break;
                    case 8:
                        if (col_text != "WholesalePrice") format_error = true;
                        break;
                    case 9:
                        if (col_text != "Title") format_error = true;
                        break;
                    case 10:
                        if ((col_text.Contains("Manufacturer") == false)) format_error = true;
                        break;
                    case 11:
                        if (col_text != "IsMurphysItem") format_error = true;
                        break;
                    case 12:
                        if (col_text != "ProductType") format_error = true;
                        break;
                    case 13:
                        if (col_text != "SuggestedRetailPrice") format_error = true;
                        break;
                    case 14:
                        if (col_text != "MaintainMSRP") format_error = true;
                        break;
                    case 15:
                        if (col_text != "ImageFileName") format_error = true;
                        break;
                    case 16:
                        if (col_text != "ImageThumbnailFileName") format_error = true;
                        break;
                    case 17:
                        if (col_text != "Videos") format_error = true;
                        break;
                    case 18:
                        if (col_text != "Categories") format_error = true; //empty
                        break;
                    case 19:
                        if (col_text != "Christmas Themed") format_error = true;
                        break;
                    case 20:
                        if (col_text != "Halloween Themed") format_error = true;
                        break;
                    case 21:
                        if (col_text != "Card Magic and Trick Decks") format_error = true;
                        break;
                    case 22:
                        if (col_text != "Decks (Custom, Standard)") format_error = true;
                        break;
                    case 23:
                        if (col_text != "Lectures and Conventions") format_error = true;
                        break;
                    case 24:
                        if (col_text != "Magazines") format_error = true;
                        break;
                    case 25:
                        if (col_text != "Money Magic") format_error = true;
                        break;
                    case 26:
                        if (col_text != "Posters, Gifts and Collectables") format_error = true;
                        break;
                    case 27:
                        if (col_text != "Refills") format_error = true;
                        break;
                    case 28:
                        if (col_text != "Silk and Silk Magic") format_error = true;
                        break;
                    case 29:
                        if (col_text != "Special Effects (Fire, Smoke, Sound)") format_error = true;
                        break;
                    case 30:
                        if (col_text != "Sponge and Sponge Magic") format_error = true;
                        break;
                    case 31:
                        if (col_text != "Tables and Cases") format_error = true;
                        break;
                    case 32:
                        if (col_text != "Theory, History and Business") format_error = true;
                        break;
                    case 33:
                        if (col_text != "Toy Magic (Toy, Kits, Puzzles)") format_error = true;
                        break;
                    case 34:
                        if (col_text != "Utility") format_error = true;
                        break;
                    case 35:
                        if (col_text != "Close Up Performer") format_error = true;
                        break;
                    case 36:
                        if (col_text != "Comedy Performer") format_error = true;
                        break;
                    case 37:
                        if (col_text != "Escape Performer") format_error = true;
                        break;
                    case 38:
                        if (col_text != "Gambling Performer") format_error = true;
                        break;
                    case 39:
                        if (col_text != "Illusionist") format_error = true;
                        break;
                    case 40:
                        if (col_text != "Juggling Performer") format_error = true;
                        break;
                    case 41:
                        if (col_text != "Kids Show and Balloon Performer") format_error = true;
                        break;
                    case 42:
                        if (col_text != "Mentalism,Bizarre and Psychokinesis Perf") format_error = true;
                        break;
                    case 43:
                        if (col_text != "Stage / Parlor Performer") format_error = true;
                        break;
                    case 44:
                        if (col_text != "Religious and Gospel Performer") format_error = true;
                        break;
                    case 45:
                        if (col_text != "Street Performer") format_error = true;
                        break;
                    case 46:
                        if (col_text != "Walk Around Performer") format_error = true;
                        break;
                    case 47:
                        if (col_text != "Black Label") format_error = true;
                        break;
                    case 48:
                        if (col_text != "Limited Edition") format_error = true;
                        break;
  
                }
            }

            if (format_error == true)
            {
                Debug.WriteLine("Error Loading File - First line not correct format");
                return null;
            }

            //Now load up the other columns
            for (int i = (worksheet.Dimension.Start.Row + 1); i <= worksheet.Dimension.End.Row; i++)
            {
                var full_image_filename = worksheet.Cells[i, 15].Value.ToString();
                var full_thumb_filename = worksheet.Cells[i, 16].Value.ToString();

                var image_filename = full_image_filename.Substring(44);
                var thumb_filename = full_image_filename.Substring(44);

                //var image_filename = worksheet.Cells[i, 18].Value.ToString().Substring(44, worksheet.Cells[i, 18].Value.ToString().Length);
                //var thumb_filename = worksheet.Cells[i, 19].Value.ToString().Substring(44, worksheet.Cells[i, 19].Value.ToString().Length);

                //Debug.WriteLine("filename: " + image_filename);
                Debug.WriteLine("Loading Data Name: " + worksheet.Cells[i, 5].Value.ToString());
                MmsItemRecord mmsItem = new MmsItemRecord();
                mmsItem.InternalId = int.Parse(worksheet.Cells[i, 2].Value.ToString());
                mmsItem.ProductId = 1;
                mmsItem.IsMmsDownload = true;
                mmsItem.ArtistOrMagician = worksheet.Cells[i, 3].Value.ToString();
                mmsItem.DateAdded = DateTime.Parse(worksheet.Cells[i, 7].Value.ToString());
                mmsItem.DateLastModified = DateTime.Now;
                mmsItem.HTMLDescription = worksheet.Cells[i, 4].Value.ToString();
                mmsItem.ImageFileName = image_filename; // Full URL not just filename
                mmsItem.ImageThumbnailFileName = thumb_filename; // Full URL not just filename
                mmsItem.InternalIdLegacy = int.Parse(worksheet.Cells[i, 2].Value.ToString());
                mmsItem.IsMurphysItem = false;
                mmsItem.ISBN = null;
                mmsItem.Manufacturer = worksheet.Cells[i, 10].Value.ToString();
                mmsItem.PreSale = false;
                mmsItem.ProductCode = worksheet.Cells[i, 1].Value.ToString();  //sku 
                mmsItem.ProductLine = null;
                mmsItem.Quality = null;
                mmsItem.QuantityAvailable = 10000;
                mmsItem.Status = 1;
                mmsItem.SuggestedRetailPrice = decimal.Parse(worksheet.Cells[i, 13].Value.ToString());
                mmsItem.Title = worksheet.Cells[i, 9].Value.ToString();
                mmsItem.Weight = 0;
                mmsItem.WholesalePrice = decimal.Parse(worksheet.Cells[i, 8].Value.ToString());
                mmsItem.Length = 0;
                mmsItem.Width = 0;
                mmsItem.Height = 0;

                if (worksheet.Cells[i, 14].Value.ToString().ToLower() == "yes")
                { mmsItem.MaintainMSRP = true; }
                else
                { mmsItem.MaintainMSRP = false; }

                // Now add collections
                // For the alt images, MMS include duplicate thimbs that are not needed in nop, so strip these out.
                // Also format is xxx,xxx-thumb | xxx | xxx.xxx-thumb

                var alt_image_list_raw = worksheet.Cells[i, 6].Value.ToString();
                if (alt_image_list_raw.Length > 1)
                {
                    string[] words = alt_image_list_raw.Split('|');
                    List<string> alt_images_nothumbs = new List<string>();
                    foreach (var word in words)
                    {
                        string[] alt_names = word.Split(',');
                        foreach (var an in alt_names)
                        {
                            var len = an.Length;
                            if ((an.Substring(len - 9) == "thumb.png") || (an.Substring(len - 9) == "thumb.jpg"))
                            {
                                continue;
                            }
                            else
                            {
                                alt_images_nothumbs.Add(an);
                            }
                        }
                    }

                    foreach (var word in alt_images_nothumbs)
                    {
                        MmsAlternateImage altImage = new MmsAlternateImage();
                        altImage.Name = word;
                        altImage.DirectName = "https://www.murphysmagicsupplies.com/images_alt/" + word;
                        altImage.ThumbnailName = null;
                        mmsItem.AlternateImages.Add(altImage);
                    }
                }

                var video_list_raw = worksheet.Cells[i, 17].Value.ToString();
                if (video_list_raw.Length > 1)
                {
                    string[] videos = video_list_raw.Split('|');

                    foreach (var vid in videos)
                    {
                        MmsVideo mmsVideo = new MmsVideo();
                        mmsVideo.Fillename = vid;
                        mmsVideo.DirectFillename = vid;
                        mmsItem.Videos.Add(mmsVideo);
                    }
                }

                // Categories

                if (worksheet.Cells[i, 19].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Christmas Themed";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 20].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Halloween Themed";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 21].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Card Magic and Trick Decks";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 22].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Decks (Custom, Standard)";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 23].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Lectures and Conventions";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 24].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Magazines";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 25].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Money Magic";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 26].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Posters, Gifts and Collectables";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 27].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Refills";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 28].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Silk and Silk Magic";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 29].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Special Effects (Fire, Smoke, Sound)";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 30].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Sponge and Sponge Magic";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 31].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Tables and Cases";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 32].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Theory, History and Business";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 33].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Toy Magic (Toy, Kits, Puzzles)";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 34].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Utility";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 35].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Close Up Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 36].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Comedy Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 37].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Escape Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 38].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Gambling Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 39].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Illusionist";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 40].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Juggling Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 41].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Kids Show and Balloon Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 42].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Mentalism,Bizarre and Psychokinesis Perf";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 43].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Stage / Parlor Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 44].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Religious and Gospel Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 45].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Street Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 46].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Walk Around Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 47].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Black Label";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 48].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Limited Edition";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
             

                mmsItemReturnList.Add(mmsItem);
            } // For each row
            Debug.WriteLine("Returning ItemList  " + mmsItemReturnList.Count.ToString());
            return mmsItemReturnList;
        } // MmsDataReadFlat

        public virtual  List<MmsItemRecord> DataReadFlatfile(Stream stream)
        {
  //          using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            List<MmsItemRecord> mmsItemReturnList = new List<MmsItemRecord>();

            //create a list to hold all the values
            List<string> excelData = new List<string>();
            bool format_error = false;

            //Firstly check that first row has valid MMS column names, if not throw an excetption
            for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
            {
                var col_text = worksheet.Cells[1, col].Value.ToString();
                Debug.WriteLine("Col Text:  " + col_text + " Col:  " + col.ToString());
                switch (col)
                {
                    case 1:
                        if (col_text != "Product Key") format_error = true;
                        break;
                    case 2:
                        if (col_text != "Legacy Product Key") format_error = true;
                        break;
                    case 3:
                        if (col_text != "Product Code") format_error = true;
                        break;
                    case 4:
                        if (col_text != "Date Added") format_error = true;
                        break;
                    case 5:
                        if (col_text != "Title") format_error = true;
                        break;
                    case 6:
                        if (col_text != "Weight") format_error = true;
                        break;
                    case 7:
                        if (col_text != "MSRP") format_error = true;
                        break;
                    case 8:
                        if (col_text != "Wholesale") format_error = true;
                        break;
                    case 9:
                        if (col_text != "Available for Web Sale") format_error = true;
                        break;
                    case 10:
                        if ((col_text.Contains("Detailed Description") == false)) format_error = true;
                        break;
                    case 11:
                        if (col_text != "Product Type") format_error = true;
                        break;
                    case 12:
                        if (col_text != "Manufacturer") format_error = true;
                        break;
                    case 13:
                        if (col_text != "Artist/Magician") format_error = true;
                        break;
                    case 14:
                        if (col_text != "Maintain MSRP") format_error = true;
                        break;
                    case 15:
                        if (col_text != "All Star Product") format_error = true;
                        break;
                    case 16:
                        if (col_text != "On Sale") format_error = true;
                        break;
                    case 17:
                        if (col_text != "SkillLevels") format_error = true;
                        break;
                    case 18:
                        if (col_text != "Image URL") format_error = true;
                        break;
                    case 19:
                        if (col_text != "Thumbnail URL") format_error = true;
                        break;
                    case 20:
                        if (col_text != "Alternate Images") format_error = true;
                        break;
                    case 21:
                        if (col_text != "Videos") format_error = true;
                        break;
                    case 22:
                        if (col_text != "Christmas Themed") format_error = true;
                        break;
                    case 23:
                        if (col_text != "Halloween Themed") format_error = true;
                        break;
                    case 24:
                        if (col_text != "Card Magic and Trick Decks") format_error = true;
                        break;
                    case 25:
                        if (col_text != "Decks (Custom, Standard)") format_error = true;
                        break;
                    case 26:
                        if (col_text != "Lectures and Conventions") format_error = true;
                        break;
                    case 27:
                        if (col_text != "Magazines") format_error = true;
                        break;
                    case 28:
                        if (col_text != "Money Magic") format_error = true;
                        break;
                    case 29:
                        if (col_text != "Posters, Gifts and Collectables") format_error = true;
                        break;
                    case 30:
                        if (col_text != "Refills") format_error = true;
                        break;
                    case 31:
                        if (col_text != "Silk and Silk Magic") format_error = true;
                        break;
                    case 32:
                        if (col_text != "Special Effects (Fire, Smoke, Sound)") format_error = true;
                        break;
                    case 33:
                        if (col_text != "Sponge and Sponge Magic") format_error = true;
                        break;
                    case 34:
                        if (col_text != "Tables and Cases") format_error = true;
                        break;
                    case 35:
                        if (col_text != "Theory, History and Business") format_error = true;
                        break;
                    case 36:
                        if (col_text != "Toy Magic (Toy, Kits, Puzzles)") format_error = true;
                        break;
                    case 37:
                        if (col_text != "Utility") format_error = true;
                        break;
                    case 38:
                        if (col_text != "Close Up Performer") format_error = true;
                        break;
                    case 39:
                        if (col_text != "Comedy Performer") format_error = true;
                        break;
                    case 40:
                        if (col_text != "Escape Performer") format_error = true;
                        break;
                    case 41:
                        if (col_text != "Gambling Performer") format_error = true;
                        break;
                    case 42:
                        if (col_text != "Illusionist") format_error = true;
                        break;
                    case 43:
                        if (col_text != "Juggling Performer") format_error = true;
                        break;
                    case 44:
                        if (col_text != "Kids Show and Balloon Performer") format_error = true;
                        break;
                    case 45:
                        if (col_text != "Mentalism,Bizarre and Psychokinesis Perf") format_error = true;
                        break;
                    case 46:
                        if (col_text != "Stage / Parlor Performer") format_error = true;
                        break;
                    case 47:
                        if (col_text != "Religious and Gospel Performer") format_error = true;
                        break;
                    case 48:
                        if (col_text != "Street Performer") format_error = true;
                        break;
                    case 49:
                        if (col_text != "Walk Around Performer") format_error = true;
                        break;
                    case 50:
                        if (col_text != "Black Label") format_error = true;
                        break;
                    case 51:
                        if (col_text != "Limited Edition") format_error = true;
                        break;
                }
            }

            if (format_error == true)
            {
                Debug.WriteLine("Error Loading File - First line not correct format");
                return null;
            }

            //Now load up the other columns
            for (int i = (worksheet.Dimension.Start.Row + 1); i <= worksheet.Dimension.End.Row; i++)
            {
                var full_image_filename = worksheet.Cells[i, 18].Value.ToString();
                var full_thumb_filename = worksheet.Cells[i, 19].Value.ToString();

                var image_filename = full_image_filename.Substring(44);
                var thumb_filename = full_image_filename.Substring(44);

                //var image_filename = worksheet.Cells[i, 18].Value.ToString().Substring(44, worksheet.Cells[i, 18].Value.ToString().Length);
                //var thumb_filename = worksheet.Cells[i, 19].Value.ToString().Substring(44, worksheet.Cells[i, 19].Value.ToString().Length);

                //Debug.WriteLine("filename: " + image_filename);
                Debug.WriteLine("Loading Data Name: " + worksheet.Cells[i, 5].Value.ToString());
                MmsItemRecord mmsItem = new MmsItemRecord();
                mmsItem.InternalId = int.Parse(worksheet.Cells[i, 1].Value.ToString());
                mmsItem.ProductId = 1;
                mmsItem.IsMmsDownload = false;
                mmsItem.ArtistOrMagician = worksheet.Cells[i, 13].Value.ToString();
                mmsItem.DateAdded = DateTime.Parse(worksheet.Cells[i, 4].Value.ToString());
                mmsItem.DateLastModified = DateTime.Now;
                mmsItem.HTMLDescription = worksheet.Cells[i, 10].Value.ToString();
                mmsItem.ImageFileName = image_filename; // Full URL not just filename
                mmsItem.ImageThumbnailFileName = thumb_filename; // Full URL not just filename
                mmsItem.InternalIdLegacy = int.Parse(worksheet.Cells[i, 2].Value.ToString());
                mmsItem.IsMurphysItem = false;
                mmsItem.ISBN = null;
                mmsItem.Manufacturer = worksheet.Cells[i, 12].Value.ToString();
                mmsItem.PreSale = false;
                mmsItem.ProductCode = worksheet.Cells[i, 3].Value.ToString();  //sku
                mmsItem.ProductLine = null;
                mmsItem.Quality = null;
                mmsItem.QuantityAvailable = int.Parse(worksheet.Cells[i, 9].Value.ToString());
                mmsItem.Status = 1;
                mmsItem.SuggestedRetailPrice = decimal.Parse(worksheet.Cells[i, 7].Value.ToString());
                mmsItem.Title = worksheet.Cells[i, 5].Value.ToString();
                mmsItem.Weight = double.Parse(worksheet.Cells[i, 6].Value.ToString());
                mmsItem.WholesalePrice = decimal.Parse(worksheet.Cells[i, 8].Value.ToString());
                mmsItem.Length = 1;
                mmsItem.Width = 1;
                mmsItem.Height = 1;

                if (worksheet.Cells[i, 14].Value.ToString().ToLower() == "yes")
                { mmsItem.MaintainMSRP = true; }
                else
                { mmsItem.MaintainMSRP = false; }

                // Now add collections
                // For the alt images, MMS include duplicate thimbs that are not needed in nop, so strip these out.
                // Also format is xxx,xxx-thumb | xxx | xxx.xxx-thumb

                var alt_image_list_raw = worksheet.Cells[i, 20].Value.ToString();
                if (alt_image_list_raw.Length > 1)
                {
                    string[] words = alt_image_list_raw.Split('|');
                    List<string> alt_images_nothumbs = new List<string>();
                    foreach (var word in words)
                    {
                        string[] alt_names = word.Split(',');
                        foreach (var an in alt_names)
                        {
                            var len = an.Length;
                            if ((an.Substring(len - 9) == "thumb.png") || (an.Substring(len - 9) == "thumb.jpg"))
                            {
                                continue;
                            }
                            else
                            {
                                alt_images_nothumbs.Add(an);
                            }
                        }
                    }

                    foreach (var word in alt_images_nothumbs)
                    {
                        MmsAlternateImage altImage = new MmsAlternateImage();
                        altImage.Name = word;
                        altImage.DirectName = "https://www.murphysmagicsupplies.com/images_alt/" + word;
                        altImage.ThumbnailName = null;
                        mmsItem.AlternateImages.Add(altImage);
                    }
                }

                var video_list_raw = worksheet.Cells[i, 21].Value.ToString();
                if (video_list_raw.Length > 1)
                {
                    string[] videos = video_list_raw.Split('|');

                    foreach (var vid in videos)
                    {
                        MmsVideo mmsVideo = new MmsVideo();
                        mmsVideo.Fillename = vid;
                        mmsVideo.DirectFillename = vid;
                        mmsItem.Videos.Add(mmsVideo);
                    }
                }

                // Categories

                if (worksheet.Cells[i, 22].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Christmas Themed";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 23].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Halloween Themed";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 24].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Card Magic and Trick Decks";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 25].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Decks (Custom, Standard)";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 26].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Lectures and Conventions";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 27].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Magazines";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 28].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Money Magic";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 29].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Posters, Gifts and Collectables";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 30].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Refills";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 31].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Silk and Silk Magic";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 32].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Special Effects (Fire, Smoke, Sound)";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 33].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Sponge and Sponge Magic";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 34].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Tables and Cases";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 35].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Theory, History and Business";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 36].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Toy Magic (Toy, Kits, Puzzles)";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 37].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Utility";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 38].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Close Up Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 39].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Comedy Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 40].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Escape Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 41].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Gambling Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 42].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Illusionist";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 43].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Juggling Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 44].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Kids Show and Balloon Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 45].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Mentalism,Bizarre and Psychokinesis Perf";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 46].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Stage / Parlor Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 47].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Religious and Gospel Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 48].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Street Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 49].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Walk Around Performer";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 50].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Black Label";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }
                if (worksheet.Cells[i, 51].Value.ToString().ToLower() == "yes")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Limited Edition";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }

                //
                if (worksheet.Cells[i, 11].Value.ToString().ToLower() == "book")
                {
                    MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();
                    mmsCat.MmsCategory = "Books";
                    mmsCat.IsNewCategory = false;
                    mmsItem.MmsCategories.Add(mmsCat);
                }

                mmsItemReturnList.Add(mmsItem);
            } // For each row
            Debug.WriteLine("Returning ItemList  " + mmsItemReturnList.Count.ToString());
            return mmsItemReturnList;
        } // MmsDataReadFlat

        protected virtual string MmsGetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);

            //set to jpeg in case mime type cannot be found
            return mimeType ?? MimeTypes.ImageJpeg;
        }

        public virtual void MmsLogPictureInsertError(string picturePath, Exception ex)
        {
            var extension = _fileProvider.GetFileExtension(picturePath);
            var name = _fileProvider.GetFileNameWithoutExtension(picturePath);

            var point = string.IsNullOrEmpty(extension) ? string.Empty : ".";
            var fileName = _fileProvider.FileExists(picturePath) ? $"{name}{point}{extension}" : string.Empty;
            _logger.Error($"Insert picture failed (file name: {fileName})", ex);
        }

        public override void ImportProductsFromXlsx(Stream stream)
        {

            Debug.WriteLine("In Overriden ImportProductsFromXlsx");

            // Firstly need to dind if the stream is for normal or download prods
            //==================


            using var xlPackage = new ExcelPackage(stream);
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            List<MmsItemRecord> mmsItemReturnList = new List<MmsItemRecord>();
            List<string> excelData = new List<string>();
            bool is_download = false;
            if (worksheet.Cells[1, 1].Value.ToString() == "Product Key")
            {
                is_download = false;
            }
            else if (worksheet.Cells[1, 1].Value.ToString() == "ProductCode")
            {
                is_download = true;
            }

            if (is_download)
            {
                var mmsItemList = DataReadDownloadFlatfile(stream);
                Debug.WriteLine("In Overriden ImportProductsFromXlsx - Read prod list");
                Debug.WriteLine(mmsItemList.Count.ToString());

                //Now loop through 100 rows at a time inserting into MMS NOP

                var page_entries = 100;
                var total_inserts = (mmsItemList.Count / 100);
                var remainder = (mmsItemList.Count % 100);
                Debug.WriteLine("total_inserts = " + total_inserts.ToString() + " remainder= " + remainder.ToString());

                for (var page = 0; page < total_inserts; page++)
                {
                    var range_from = page * page_entries;
                    var range_to = ((page * page_entries) + (page_entries - 1));
                    Debug.WriteLine("range_from = " + range_from.ToString() + " range_to= " + range_to.ToString());
                    var mmsInsertList = mmsItemList.GetRange(range_from, page_entries);

                    if (mmsInsertList != null)
                    {
                        Debug.WriteLine("Calling mmsdatainsert2 :" + mmsInsertList.Count.ToString());
                        MmsDataImportInsert2(mmsInsertList);
                    }

                }
                var range_from2 = total_inserts * page_entries;
                var range_t2 = (total_inserts * page_entries) + remainder;
                var mmsInsertList2 = mmsItemList.GetRange(range_from2, remainder);
                Debug.WriteLine("Calling final mmsdatainsert2 :" + mmsInsertList2.Count.ToString());
                Debug.WriteLine("Final range_from = " + range_from2.ToString() + " range_to= " + range_t2.ToString());

                if (mmsInsertList2 != null)
                {
                    MmsDataImportInsert2(mmsInsertList2);
                }
            }
            else
            {
                var mmsItemList = DataReadFlatfile(stream);
                Debug.WriteLine("In Overriden ImportProductsFromXlsx - Read prod list");
                Debug.WriteLine(mmsItemList.Count.ToString());

                //Now loop through 100 rows at a time inserting into MMS NOP

                var page_entries = 100;
                var total_inserts = (mmsItemList.Count / 100);
                var remainder = (mmsItemList.Count % 100);
                Debug.WriteLine("total_inserts = " + total_inserts.ToString() + " remainder= " + remainder.ToString());

                for (var page = 0; page < total_inserts; page++)
                {
                    var range_from = page * page_entries;
                    var range_to = ((page * page_entries) + (page_entries - 1));
                    Debug.WriteLine("range_from = " + range_from.ToString() + " range_to= " + range_to.ToString());
                    var mmsInsertList = mmsItemList.GetRange(range_from, page_entries);

                    if (mmsInsertList != null)
                    {
                        Debug.WriteLine("Calling mmsdatainsert2 :" + mmsInsertList.Count.ToString());
                        MmsDataImportInsert2(mmsInsertList);
                    }

                }
                var range_from2 = total_inserts * page_entries;
                var range_t2 = (total_inserts * page_entries) + remainder;
                var mmsInsertList2 = mmsItemList.GetRange(range_from2, remainder);
                Debug.WriteLine("Calling final mmsdatainsert2 :" + mmsInsertList2.Count.ToString());
                Debug.WriteLine("Final range_from = " + range_from2.ToString() + " range_to= " + range_t2.ToString());

                if (mmsInsertList2 != null)
                {
                    MmsDataImportInsert2(mmsInsertList2);
                }
            }
            

 

        }

        public virtual void MmsDataImportInsert2(List<MmsItemRecord> mmsDataList)
        {
            // Get List of SKUs
            List<string> allSkuList = new List<string>();

            foreach (var mmsDataItem in mmsDataList)
            {
                if (mmsDataItem.ProductCode != null)
                {
                    allSkuList.Add(mmsDataItem.ProductCode);
                }            
            }
            var allProductsBySku = _productService.GetProductsBySku(allSkuList.ToArray(), 0);

            //performance optimization, load all categories IDs for products in one SQL request
            var allProductsCategoryIds = _categoryService.GetProductCategoryIds(allProductsBySku.Select(p => p.Id).ToArray());

            //performance optimization, load all categories in one SQL request
            Dictionary<CategoryKey, Category> allCategories;
            try
            {
                var allCategoryList = _categoryService.GetAllCategories(showHidden: true);

                allCategories = allCategoryList
                    .ToDictionary(c => new CategoryKey(c, _categoryService, allCategoryList, _storeMappingService), c => c);
            }
            catch (ArgumentException)
            {
                //categories with the same name are not supported in the same category level
                throw new ArgumentException(_localizationService.GetResource("Admin.Catalog.Products.Import.CategoriesWithSameNameNotSupported"));
            }

            //product to import images
            var productPictureMetadata = new List<ProductPictureMetadata>();

            //Product lastLoadedProduct = null;

            foreach (var mmsDataItem in mmsDataList)
            {
                var productToImport = allProductsBySku.FirstOrDefault(p => p.Sku == mmsDataItem.ProductCode);

                var isNew = productToImport == null;

                productToImport ??= new Product();

                //some of previous values
                var previousStockQuantity = productToImport.StockQuantity;
                var previousWarehouseId = productToImport.WarehouseId;

                if (isNew)
                {
                    productToImport.CreatedOnUtc = DateTime.UtcNow;
                    productToImport.OldPrice = 0;
                }
                else
                {
                    productToImport.OldPrice = productToImport.Price;
                }

                productToImport.ProductType = ProductType.SimpleProduct;
                productToImport.ProductTypeId = 5; //Check
                productToImport.ParentGroupedProductId = 0;
                productToImport.VisibleIndividually = true;
                productToImport.Name = mmsDataItem.Title;
                productToImport.FullDescription = mmsDataItem.HTMLDescription;
                productToImport.Sku = mmsDataItem.ProductCode;
                productToImport.ShortDescription = mmsDataItem.Title;
                productToImport.AdminComment = null;
                productToImport.VendorId = 0;
                productToImport.MetaKeywords = mmsDataItem.Title;
                productToImport.MetaDescription = mmsDataItem.Title;
                productToImport.MetaTitle = mmsDataItem.Title;
                productToImport.ShowOnHomepage = false;
                productToImport.AllowCustomerReviews = true;
                productToImport.ApprovedRatingSum = 0;
                productToImport.NotApprovedRatingSum = 0;
                productToImport.ApprovedTotalReviews = 0;
                productToImport.NotApprovedTotalReviews = 0;
                productToImport.SubjectToAcl = false;
                productToImport.LimitedToStores = false;
                productToImport.ManufacturerPartNumber = mmsDataItem.ProductCode; //sku
                productToImport.Gtin = null;
                productToImport.IsGiftCard = false;
                productToImport.GiftCardTypeId = 0;
                productToImport.OverriddenGiftCardAmount = null;
                productToImport.RequireOtherProducts = false;
                productToImport.RequiredProductIds = null;
                productToImport.AutomaticallyAddRequiredProducts = false;
                productToImport.IsDownload = false;
                productToImport.DownloadId = 0;
                productToImport.UnlimitedDownloads = true;
                productToImport.MaxNumberOfDownloads = 10000;
                productToImport.DownloadExpirationDays = null;
                productToImport.DownloadActivationTypeId = 0;
                productToImport.HasSampleDownload = false;
                productToImport.SampleDownloadId = 0;
                productToImport.HasUserAgreement = false;
                productToImport.UserAgreementText = null;
                productToImport.IsRecurring = false;
                productToImport.RecurringCycleLength = 100;
                productToImport.RecurringCyclePeriodId = 0;
                productToImport.RecurringTotalCycles = 10;
                productToImport.IsRental = false;
                productToImport.RentalPriceLength = 1;
                productToImport.RentalPricePeriodId = 0;
                productToImport.IsShipEnabled = true;
                productToImport.IsFreeShipping = false;
                productToImport.ShipSeparately = false;
                productToImport.AdditionalShippingCharge = 0;
                productToImport.DeliveryDateId = 0;
                productToImport.IsTaxExempt = false;
                productToImport.TaxCategoryId = 0;
                productToImport.IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
                productToImport.ManageInventoryMethodId = 1; // check on this
                productToImport.ProductAvailabilityRangeId = 0;
                productToImport.UseMultipleWarehouses = false;
                productToImport.WarehouseId = 0;
                productToImport.StockQuantity = mmsDataItem.QuantityAvailable;
                productToImport.DisplayStockAvailability = true;
                productToImport.DisplayStockQuantity = true;
                productToImport.MinStockQuantity = 0;
                productToImport.LowStockActivityId = 0;
                productToImport.NotifyAdminForQuantityBelow = 1;
                productToImport.BackorderModeId = 0; // check on this, might be 1
                productToImport.AllowBackInStockSubscriptions = false;
                productToImport.OrderMinimumQuantity = 1;
                productToImport.OrderMaximumQuantity = 10000;
                productToImport.AllowedQuantities = null;
                productToImport.AllowAddingOnlyExistingAttributeCombinations = false;
                productToImport.NotReturnable = false;
                productToImport.DisableBuyButton = false;
                productToImport.DisableWishlistButton = false;
                productToImport.AvailableForPreOrder = mmsDataItem.PreSale;
                productToImport.PreOrderAvailabilityStartDateTimeUtc = DateTime.Now;
                productToImport.CallForPrice = false;
                productToImport.ProductCost = mmsDataItem.WholesalePrice;
                productToImport.CustomerEntersPrice = false;
                productToImport.MinimumCustomerEnteredPrice = 0;
                productToImport.MaximumCustomerEnteredPrice = 1000;
                productToImport.BasepriceEnabled = false;
                productToImport.BasepriceAmount = 0;
                productToImport.BasepriceUnitId = 1;
                productToImport.BasepriceBaseAmount = 0;
                productToImport.BasepriceBaseUnitId = 1;
                productToImport.MarkAsNew = true;
                productToImport.MarkAsNewStartDateTimeUtc = null;
                productToImport.MarkAsNewEndDateTimeUtc = null;
                productToImport.HasTierPrices = false;
                productToImport.HasDiscountsApplied = false;
                productToImport.Weight = (decimal)mmsDataItem.Weight;
                productToImport.Length = (decimal)mmsDataItem.Length;
                productToImport.Width = (decimal)mmsDataItem.Width;
                productToImport.Height = (decimal)mmsDataItem.Height;
                productToImport.AvailableStartDateTimeUtc = null;
                productToImport.AvailableEndDateTimeUtc = null;
                productToImport.DisplayOrder = 0;
                productToImport.Published = true;
                productToImport.Deleted = false;
                productToImport.UpdatedOnUtc = DateTime.UtcNow;
                productToImport.CreatedOnUtc = DateTime.UtcNow;
                productToImport.Price = MmSPriceCalc(mmsDataItem.WholesalePrice, false);


                if (mmsDataItem.Videos.Count == 0)
                {
                    productToImport.ProductTemplateId = 1; //Simple Template, no video
                }
                else
                {
                    productToImport.ProductTemplateId = 3; //Simple Template, with video
                }

                if (isNew)
                {
                    Debug.WriteLine("Inserting Product " + productToImport.Name);
                    _productService.InsertProduct(productToImport);
                }
                else
                {
                    Debug.WriteLine("Updating Product " + productToImport.Name);
                    _productService.UpdateProduct(productToImport);
                }

                // Handle Videos
                string mms_new_video_full;
                string mms_new_video_string;
                string mms_video_string;

                if (mmsDataItem.Videos.Count == 0)
                {
            
                    mms_new_video_full = "no_video";
                }
                else
                {
                    mms_video_string = mmsDataItem.Videos.FirstOrDefault().Fillename;
                    mms_new_video_string = Path.ChangeExtension(mms_video_string, ".mp4");
                    mms_new_video_full = "https://www.murphysmagicsupplies.com/video/clips_mp4fs/" + mms_new_video_string;
                }

                // Now do the videos
                int ret_i = _mmsAdminService.InsertVideo(productToImport.Id, mms_new_video_full, mmsDataItem.InternalId);

                //search engine name            
                var seName = productToImport.Name;
                _urlRecordService.SaveSlug(productToImport, _urlRecordService.ValidateSeName(productToImport, seName, productToImport.Name, true), 0);

                var newPictureP = MmsLoadPicture(mmsDataItem.ImageFileName, productToImport.Name, null);

                // Handle Pictures, first the main one

                PictureMetadata pic_meta = new PictureMetadata();
                List<PictureMetadata> pic_list = new List<PictureMetadata>();

                //List<string> picture_new_list = new List<string>();
                //picture_new_list.Add(mmsDataItem.ImageFileName);
                pic_meta.PictureName = mmsDataItem.ImageFileName;
                pic_meta.PicturePath = "https://www.murphysmagicsupplies.com/images/" + mmsDataItem.ImageFileName;
                pic_list.Add(pic_meta);

                Debug.WriteLine("In insert, 455 {0}  {1}", productToImport.Name, pic_list.Count.ToString());
                if (pic_list.Count > 0)
                {
                    MmsImportProductImagesUsingServices(productToImport, pic_list, 1, isNew);
                }

                // Now the alt images


                List<PictureMetadata> alt_pic_list = new List<PictureMetadata>();
                foreach (var alt_pic in mmsDataItem.AlternateImages)
                {
                    PictureMetadata alt_pic_meta = new PictureMetadata();
                    if (alt_pic != null)
                    {
                        alt_pic_meta.PictureName = alt_pic.Name;
                        alt_pic_meta.PicturePath = "https://www.murphysmagicsupplies.com/images_alt/" + alt_pic.Name;
                        alt_pic_list.Add(alt_pic_meta);
                    }
                }
                if (alt_pic_list != null)
                {
                    MmsImportProductImagesUsingServices(productToImport, alt_pic_list, 2, isNew);
                }

                // Categories
                // Add categoroies that do not exist

                var allCategoriesNames = new List<NopCatMap>();

                /*foreach (var mms.Category in mmsDataItem.MmsCategories)
                {
                    // Check if Category exists
                    // writer.WriteLine(" 414 MmsCatName: " + mmsCategory.MmsCategory);
                    var nop_cat_map = mmsCategoryMap(mmsCategory.MmsCategory);
                    allCategoriesNames.Add(nop_cat_map);
                    catNameList.Add(nop_cat_map.CatName);
                }*/
                var catNameList = new List<string>();
                foreach (var mmsCategory in mmsDataItem.MmsCategories)
                {
                    catNameList.Add(mmsCategory.MmsCategory);
                }

                //performance optimization, the check for the existence of the categories in one SQL request
                var notExistingCategories = _categoryService.GetNotExistingCategories(catNameList.ToArray());
                /*if (notExistingCategories.Any())
                {
                    throw new ArgumentException(string.Format(_localizationService.GetResource("Admin.Catalog.Products.Import.CategoriesDontExist"), string.Join(", ", notExistingCategories)));
                }*/

                // Insert new categories
                if (notExistingCategories != null)
                {
                    foreach (var non_exist_cat in notExistingCategories)
                    {
                        var newCategory = new Category();
                        newCategory.Name = non_exist_cat;
                        newCategory.Description = non_exist_cat;
                        newCategory.CategoryTemplateId = 1;
                        newCategory.MetaKeywords = non_exist_cat;
                        newCategory.MetaDescription = non_exist_cat;
                        newCategory.MetaTitle = non_exist_cat;
                        newCategory.ParentCategoryId = 0;
                        newCategory.PictureId = 0;
                        newCategory.PageSize = 6;
                        newCategory.AllowCustomersToSelectPageSize = true;
                        newCategory.PageSizeOptions = "10, 30, 60";
                        newCategory.PriceRanges = null;
                        newCategory.ShowOnHomepage = false;
                        newCategory.IncludeInTopMenu = true;
                        newCategory.SubjectToAcl = false;
                        newCategory.LimitedToStores = false;
                        newCategory.Published = true;
                        newCategory.Deleted = false;
                        newCategory.DisplayOrder = 1;
                        newCategory.CreatedOnUtc = DateTime.Now;
                        newCategory.UpdatedOnUtc = DateTime.Now;

                        _categoryService.InsertCategory(newCategory);

                        _urlRecordService.SaveSlug(newCategory, _urlRecordService.ValidateSeName(newCategory, newCategory.Name, newCategory.Name, true), 0);
                    }
                }

                //Now add categories to product
                // Get CategoryIds

                var nopCatListAdd = new List<Category>();
                var nopCatIdListAdd = new List<int>();
                var all_cats = _categoryService.GetAllCategories(0, false);
                foreach (var cat in all_cats)
                {
                    foreach (var mms_cat in catNameList)
                    {
                        if (cat.Name == mms_cat)
                        {
                            nopCatListAdd.Add(cat);
                            nopCatIdListAdd.Add(cat.Id);
                        }
                    }
                }

                var existing_prodcats = _categoryService.GetProductCategoriesByProductId(productToImport.Id);
                var existingCatIds = new List<int>();
                foreach (var ex_pc in existing_prodcats)
                {
                    existingCatIds.Add(ex_pc.CategoryId);
                }
                 

                foreach (var cat in nopCatListAdd)
                {
                    if (existingCatIds.Contains(cat.Id) == false)
                    {
                        var productCategory = new ProductCategory
                        {
                            ProductId = productToImport.Id,
                            CategoryId = cat.Id,
                            IsFeaturedProduct = false,
                            DisplayOrder = 1
                        };
                        _categoryService.InsertProductCategory(productCategory);
                    }
                }

                // Delete not wanted product categories

                var prod_cats = _categoryService.GetProductCategoriesByProductId(productToImport.Id, true);

                foreach (var prod_cat in prod_cats)
                {
                    if (nopCatIdListAdd.Contains(prod_cat.CategoryId) == false)
                    {
                        _categoryService.DeleteProductCategory(prod_cat);
                    }
                }

                // WGR 14-11-19 Add to books start
                /*string book_text = "Book";
                if (productToImport.Name.Contains(book_text))
                {
                    productToImport.ProductCategories.Add(new ProductCategory
                    {
                        CategoryId = 34, //Books
                        DisplayOrder = cat_display_order,
                        IsFeaturedProduct = false
                    });
                    cat_display_order++;

                    _productService.UpdateProduct(productToImport);
                }*/
                // WGR 14-11-19 Add to books end
            }
        }
    }
}
