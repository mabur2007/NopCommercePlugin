using System;
using System.Collections.Generic;
using V4;
using System.Diagnostics;
using Nop.Plugin.Misc.MmsAdmin.Domain;

namespace Nop.Plugin.Misc.MmsAdmin
{

    class MmsDataRead
    {
        public List<MmsItemRecord> DataReadFileBetweenPaged(DateTime from_date, DateTime until_date, string user_name, string password, int mms_page)
        {

            string Username = user_name;
            string Password = password;
            int page = 1;


            V4.SoapAuthenticationHeader authentication = new V4.SoapAuthenticationHeader();


            Debug.WriteLine(">>>>In DataReadFileBetweenPaged");

             authentication.Username = Username;
             authentication.Password = Password;


             GetHelloRequest request = new GetHelloRequest();
             request.SoapAuthenticationHeader = authentication;
             V4.V4SoapClient client = new V4SoapClient(V4SoapClient.EndpointConfiguration.V4Soap12);



            //string Username1 = "bill@davenportsmagic.co.uk";
            //string Password1 = "mA1389giC";

            int item_count = 0;

            List<MmsItemRecord> mmsItemReturnList = new List<MmsItemRecord>();

            GetHelloRequest helloRequest = new GetHelloRequest();
            helloRequest.SoapAuthenticationHeader = authentication;

            // Output
            string hello;

            var response_hello = client.GetHello(authentication, out hello);

            Debug.WriteLine(response_hello.ToString());
            Debug.WriteLine(hello);

            //==================
            //string dateInString = "01.10.2000";
            //DateTime StartDate = DateTime.Parse(dateInString);

            Debug.WriteLine(from_date.ToString());
            Debug.WriteLine(until_date.ToString());

            // Output
            V4.InventoryItem[] items;

 
            var IABresponse = client.GetInventoryItemsAddedBetween(authentication, DateTime.Now.AddDays(-10), DateTime.Now, 1, out items);

            Debug.WriteLine("Added >>>");
            Debug.WriteLine(items.Length.ToString());
            Debug.WriteLine(IABresponse.Success.ToString());

            if ((IABresponse.Success == true) &&
                (items.Length > 0))
            {
                Debug.WriteLine(">>>> Page : " + page.ToString() + " items: " + items.Length.ToString());

                for (int i = 0; i < items.Length; i++)
                {
                    Debug.WriteLine(items[i].Title);

                    //++++++++++++++++++++++++++++++++++++++
                    MmsItemRecord mmsItem = new MmsItemRecord();

                    mmsItem.InternalId = items[i].InternalId;
                    mmsItem.ProductId = 1;
                    mmsItem.IsMmsDownload = false;
                    mmsItem.ArtistOrMagician = items[i].ArtistOrMagician;
                    mmsItem.DateAdded = items[i].DateAdded;
                    mmsItem.DateLastModified = items[i].DateLastModified;
                    mmsItem.HTMLDescription = items[i].HTMLDescription;
                    mmsItem.ImageFileName = items[i].ImageFileName;
                    mmsItem.ImageThumbnailFileName = items[i].ImageThumbnailFileName;
                    mmsItem.InternalIdLegacy = items[i].InternalIdLegacy;
                    mmsItem.IsMurphysItem = items[i].IsMurphysItem;
                    mmsItem.ISBN = items[i].ISBN;
                    mmsItem.MaintainMSRP = items[i].MaintainMSRP;
                    mmsItem.Manufacturer = items[i].Manufacturer;
                    mmsItem.PreSale = items[i].PreSale;
                    mmsItem.ProductCode = items[i].ProductCode.Trim();
                    mmsItem.ProductLine = items[i].ProductLine;
                    mmsItem.Quality = items[i].Quality;
                    mmsItem.QuantityAvailable = items[i].QuantityAvailable;
                    mmsItem.Status = items[i].Status;
                    mmsItem.SuggestedRetailPrice = items[i].SuggestedRetailPrice;
                    mmsItem.Title = items[i].Title;
                    mmsItem.Weight = items[i].Weight;
                    mmsItem.WholesalePrice = items[i].WholesalePrice;
                    mmsItem.Length = items[i].Length;
                    mmsItem.Width = items[i].Width;
                    mmsItem.Height = items[i].Height;

                    // Now add collections

                    if (items[i].AlternateImages != null)
                    {
                        for (int j = 0; j < items[i].AlternateImages.Length; j++)
                        {
                            if (items[i].AlternateImages[j].Name != null)
                            {
                                MmsAlternateImage altImage = new MmsAlternateImage();
                                altImage.Name = items[i].AlternateImages[j].Name;
                                altImage.DirectName = "https://www.murphysmagicsupplies.com/images_alt/" + items[i].AlternateImages[j].Name;
                                altImage.ThumbnailName = items[i].AlternateImages[j].ThumbnailName;
                                mmsItem.AlternateImages.Add(altImage);

                                Debug.WriteLine(" Inside Adding alt images Item: {0} >>  {1}", items[i].Title, items[i].AlternateImages[j].Name);
                            }
                        }
                    }

                    if (items[i].Videos != null)
                    {
                        for (int j = 0; j < items[i].Videos.Length; j++)
                        {
                            MmsVideo mmsVideo = new MmsVideo();
                            mmsVideo.Fillename = items[i].Videos[j].Filename;
                            mmsVideo.DirectFillename = items[i].Videos[j].Filename;

                            mmsItem.Videos.Add(mmsVideo);
                        }
                    }

                    if (items[i].WholesalePriceLevels != null)
                    {
                        for (int j = 0; j < items[i].WholesalePriceLevels.Length; j++)
                        {
                            MmsWholesalePriceLevel mmsWPL = new MmsWholesalePriceLevel();
                            mmsWPL.Price = items[i].WholesalePriceLevels[j].Price;
                            mmsWPL.Quantity = items[i].WholesalePriceLevels[j].Quantity;

                            mmsItem.MmsWholesalePriceLevels.Add(mmsWPL);
                        }
                    }

                    if (items[i].Categories != null)
                    {
                        for (int j = 0; j < items[i].Categories.Length; j++)
                        {
                            MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();

                            mmsCat.IsNewCategory = false;
                            mmsCat.MmsCategory = items[i].Categories[j];

                            mmsItem.MmsCategories.Add(mmsCat);
                        }
                    }

                    if (items[i].CategoriesNew != null)
                    {
                        for (int j = 0; j < items[i].CategoriesNew.Length; j++)
                        {
                            MmsCategoriesRecord mmsCat = new MmsCategoriesRecord();

                            mmsCat.IsNewCategory = true;
                            mmsCat.MmsCategory = items[i].CategoriesNew[j];

                            mmsItem.MmsCategories.Add(mmsCat);
                        }
                    }


                    if (items[i].Rating != null)
                    {

                        for (int j = 0; j < items[i].Rating.Length; j++)
                        {
                            MmsRateRecord mmsRate = new MmsRateRecord();

                            mmsRate.MmsRate = items[i].Rating[j];

                            mmsItem.MmsRates.Add(mmsRate);
                        }
                    }


                    if (items[i].RelatedProducts != null)
                    {
                        for (int j = 0; j < items[i].RelatedProducts.Length; j++)
                        {
                            MmsRelatedProductsRecord mmsRel = new MmsRelatedProductsRecord();

                            mmsRel.MmsRelatedItemRecord = items[i].RelatedProducts[j];

                            mmsItem.MmsRelatedProduct.Add(mmsRel);
                        }
                    }

                    if (items[i].Tags != null)
                    {
                        for (int j = 0; j < items[i].Tags.Length; j++)
                        {
                            MmsTagsRecord mmsTag = new MmsTagsRecord();

                            mmsTag.MmsTags = items[i].Tags[j];

                            mmsItem.MmsTags.Add(mmsTag);
                        }
                    }

                    Debug.WriteLine("Item Count " + item_count.ToString());
                    item_count++;

                    mmsItemReturnList.Add(mmsItem);

                }

            }
            else
            {
                Debug.WriteLine("No items fetched");
            }

            return mmsItemReturnList;
        } // DataReadFileBetweenPaged   

    } //class MmsDataRead


}//Namespace
