using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Sandbox.Import.Promotion;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SBInventoryPromotionController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private PromotionFactory _factory = null;
        private ProductFactory _factoryProduct = null;

        List<string> listPropertyReject = null;

        public void PropertyReject()
        {
            foreach (var item in listPropertyReject)
            {
                if (ModelState.ContainsKey(item))
                    ModelState[item].Errors.Clear();
            }
        }

        public SBInventoryPromotionController()
        {
            _factory = new PromotionFactory();
            _factoryProduct = new ProductFactory();

            ViewBag.ListStore = GetListStore();

            listPropertyReject = new List<string>();
            listPropertyReject.Add("DiscountType");
        }

        public ActionResult Index()
        {
            try
            {
                PromotionViewModels model = new PromotionViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Promotion_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(PromotionViewModels model)
        {
            try
            {
                var data = _factory.GetListPromotion(model.StoreID, CurrentUser.ListOrganizationId);
                foreach (var item in data)
                {
                    item.ImageURL = string.IsNullOrEmpty(item.ImageURL) ? Commons.Image100_100 : item.ImageURL;
                }
                model.ListItem = data;
            }
            catch (Exception e)
            {
                _logger.Error("Promotion_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
            return PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            PromotionModels model = new PromotionModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(PromotionModels model)
        {
            try
            {
                byte[] photoByte = null;
                PropertyReject();
                //# Image
                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                    photoByte = imgByte;
                }

                //# Unlimited Maximum Aearn Amout
                if (model.IsLimited.HasValue)
                {
                    if (model.IsLimited.Value)
                    {
                        model.MaximumEarnAmount = null;
                    }
                    else
                    {
                        if (model.MaximumEarnAmount < 0)
                            ModelState.AddModelError("MaximumEarnAmount", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                    }
                }

                //# Apply From
                if (model.FromDate.Value > model.ToDate.Value)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));

                //# Unlimited Promotion Time
                if (model.IsLimitedTime)
                {
                    model.FromTime = null;
                    model.ToTime = null;
                }
                else
                {
                    model.FromTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, model.TStartTime.Hours, model.TStartTime.Minutes, model.TStartTime.Days);
                    model.ToTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, model.TEndTime.Hours, model.TEndTime.Minutes, model.TEndTime.Days);
                    //==========
                    //if (model.FromDate.Value.Date == model.ToDate.Value.Date)
                    {
                        if (model.FromTime.Value.TimeOfDay >= model.ToTime.Value.TimeOfDay)
                        {
                            ModelState.AddModelError("IsLimitedTime", CurrentUser.GetLanguageTextFromKey("From time must be less than To time"));
                        }
                    }
                }

                //# Promotion Date Type
                if (model.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                {
                    if (model.ListWeekDayV2.Count(x => x.Status == (byte)Commons.EStatus.Deleted) == 7)
                        ModelState.AddModelError("RepeatType", CurrentUser.GetLanguageTextFromKey("Please select specific days in a week"));
                    else
                        model.DateOfWeek = string.Join("-", model.ListWeekDayV2.Where(x => x.Status != (byte)Commons.EStatus.Deleted).Select(x => x.Index).ToList());
                }
                else if (model.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                {
                    if (model.ListMonthDayV2.Count(x => x.Status == (byte)Commons.EStatus.Deleted) == 31)
                        ModelState.AddModelError("RepeatType", CurrentUser.GetLanguageTextFromKey("Please select specific days in a month"));
                    else
                        model.DateOfMonth = string.Join("-", model.ListMonthDayV2.Where(x => x.Status != (byte)Commons.EStatus.Deleted).Select(x => x.Index).ToList());
                }

                //# Spending
                if (model.ListSpendingRule != null)
                {
                    model.ListSpendingRule = model.ListSpendingRule.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                    if (model.ListSpendingRule.Count > 1)
                        model.isSpendOperatorAnd = CurrentUser.GetLanguageTextFromKey(model.ListSpendingRule.LastOrDefault().Condition).Equals(CurrentUser.GetLanguageTextFromKey("AND")) ? true : false;

                    int index = 1;
                    foreach (var item in model.ListSpendingRule)
                    {
                        if (item.SpendOnType == (byte)Commons.ESpendOnType.SpecificItem)
                        {
                            if (item.ListProduct.Count == 0)
                            {
                                ModelState.AddModelError("Spending", CurrentUser.GetLanguageTextFromKey("Please select specific items in item detail for spending rule no.") + index);
                                break;
                            }
                        }
                        if (item.Amount < 0)
                        {
                            ModelState.AddModelError("Spending", CurrentUser.GetLanguageTextFromKey("Please enter value of Quantity/Amount for spending rule no.") + index + "");
                            break;
                        }
                        else
                        {
                            item.Amount = Math.Round(item.Amount, 2);
                        }
                        index++;
                    }
                }
                else
                    ModelState.AddModelError("Spending", CurrentUser.GetLanguageTextFromKey("The Promotion must have at least one spending rule"));

                //# Earning
                if (model.ListEarningRule != null)
                {
                    model.ListEarningRule = model.ListEarningRule.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                    if (model.ListEarningRule.Count > 1)
                        model.isEarnOperatorAnd = CurrentUser.GetLanguageTextFromKey(model.ListEarningRule.LastOrDefault().Condition).Equals(CurrentUser.GetLanguageTextFromKey("AND")) ? true : false;
                    int index = 1;
                    foreach (var item in model.ListEarningRule)
                    {
                        if (item.EarnType == (byte)Commons.EEarnType.SpecificItem)
                        {
                            if (item.ListProduct.Count == 0)
                            {
                                ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("Please select specific items in item detail for earning rule no.") + index);
                                break;
                            }
                        }
                        //=======
                        if (item.bDiscountType == (byte)Commons.EValueType.Percent)
                        {
                            if (item.DiscountValue < 0 || item.DiscountValue > 100)
                            {
                                ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("Discount Percent could not larger than 100%"));
                                break;
                            }
                        }
                        else
                        {
                            if (item.DiscountValue < 0)
                            {
                                ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("Please enter value of Percent/Value for earning rule no.") + index + "");
                                break;
                            }
                            else
                            {
                                item.DiscountValue = Math.Round(item.DiscountValue, 2);
                            }
                        }
                        if (item.Quantity < 0)
                        {
                            ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("Please enter value of Quantity for earning rule no.") + index + "");
                            break;
                        }
                        //=====
                        item.DiscountType = item.bDiscountType == (byte)Commons.EValueType.Percent ? false : true;
                        index++;
                    }
                }
                else
                    ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("The Promotion must have at least one earnin rule"));

                //============
                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    return View(model);
                }
                //====================
                string msg = "";
                bool result = _factory.InsertOrUpdatePromotion(model, ref msg);
                if (result)
                {
                    //Save Image on Server
                    if (!string.IsNullOrEmpty(model.ImageURL) && model.PictureByte != null)
                    {
                        var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                        var path = string.Format("{0}{1}", originalDirectory, model.ImageURL);
                        MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                        ms.Write(photoByte, 0, photoByte.Length);
                        System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                        ImageHelper.Me.SaveCroppedImage(imageTmp, path, model.ImageURL, ref photoByte);
                        model.PictureByte = photoByte;

                        FTP.Upload(model.ImageURL, model.PictureByte);

                        ImageHelper.Me.TryDeleteImageUpdated(path);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Name", msg);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Promotion_Create: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public PromotionModels GetDetail(string id)
        {
            try
            {
                PromotionModels model = _factory.GetPromotion(null, id);
                if (model.FromTime == null || model.ToTime == null)
                    model.IsLimitedTime = true;
                else
                {
                    model.IsLimitedTime = false;
                    model.TStartTime = model.FromTime.Value.ToLocalTime().TimeOfDay;
                    model.TEndTime = model.ToTime.Value.ToLocalTime().TimeOfDay;
                }

                model.FromDate = model.FromDate.Value.ToLocalTime();
                model.ToDate = model.ToDate.Value.ToLocalTime();

                //===========
                model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                if (model.MaximumEarnAmount.HasValue)
                    model.IsLimited = false;
                else
                    model.IsLimited = true;
                //======
                if (!string.IsNullOrEmpty(model.DateOfWeek))
                {
                    model.RepeatType = 2;
                    model.ListWeekDayV2.ForEach(x =>
                    {
                        if (model.DateOfWeek.Split('-').Contains(x.Index.ToString()))
                        {
                            x.IsActive = true;
                            x.Status = 1;
                        }
                    });
                }
                else if (!string.IsNullOrEmpty(model.DateOfMonth))
                {
                    model.RepeatType = 3;
                    model.ListMonthDayV2.ForEach(x =>
                    {
                        if (model.DateOfMonth.Split('-').Contains(x.Index.ToString()))
                        {
                            x.IsActive = true;
                            x.Status = 1;
                        }
                    });
                }
                //=========Spending
                if (model.ListSpendingRule != null)
                {
                    for (int i = 0; i < model.ListSpendingRule.Count; i++)
                    {
                        var item = model.ListSpendingRule[i];
                        item.OffSet = i;
                        if (item.ListProduct != null)
                        {
                            string ItemDetail = "";
                            for (int j = 0; j < item.ListProduct.Count; j++)
                            {
                                item.ListProduct[j].OffSet = j;
                                ItemDetail += item.ListProduct[j].Name + ",";
                            }
                            item.ItemDetail = ItemDetail;
                        }
                        if (i > 0)
                        {
                            item.Condition = model.isSpendOperatorAnd ? CurrentUser.GetLanguageTextFromKey("AND") : CurrentUser.GetLanguageTextFromKey("OR");
                        }
                    }
                }
                //======Earning
                if (model.ListEarningRule != null)
                {
                    for (int i = 0; i < model.ListEarningRule.Count; i++)
                    {
                        var item = model.ListEarningRule[i];
                        item.OffSet = i;
                        if (item.ListProduct != null)
                        {
                            string ItemDetail = "";
                            for (int j = 0; j < item.ListProduct.Count; j++)
                            {
                                item.ListProduct[j].OffSet = j;
                                ItemDetail += item.ListProduct[j].Name + ",";
                            }
                            item.ItemDetail = ItemDetail;
                        }
                        if (!item.DiscountType)
                        {
                            item.bDiscountType = (byte)Commons.EValueType.Percent;
                        }
                        else
                        {
                            item.bDiscountType = (byte)Commons.EValueType.Currency;
                        }
                        //=======
                        if (i > 0)
                        {
                            item.Condition = model.isEarnOperatorAnd ? CurrentUser.GetLanguageTextFromKey("AND") : CurrentUser.GetLanguageTextFromKey("OR");
                        }
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Promotion_Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public PartialViewResult View(string id)
        {
            PromotionModels model = GetDetail(id);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string id)
        {
            PromotionModels model = GetDetail(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(PromotionModels model)
        {
            try
            {
                byte[] photoByte = null;
                PropertyReject();
                //# Image           
                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                    photoByte = imgByte;
                }
                else if (!string.IsNullOrEmpty(model.ImageURL))
                    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                //# Unlimited Maximum Aearn Amout
                if (model.IsLimited.HasValue)
                {
                        if (model.IsLimited.Value)
                        model.MaximumEarnAmount = null;
                    else
                    {
                        if (model.MaximumEarnAmount < 0)
                            ModelState.AddModelError("MaximumEarnAmount", CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0"));
                    }
                }

                //# Apply From
                if (model.FromDate.Value > model.ToDate.Value)
                    ModelState.AddModelError("FromDate", CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date."));
                //# Unlimited Promotion Time
                if (model.IsLimitedTime)
                {
                    model.FromTime = null;
                    model.ToTime = null;
                }
                else
                {
                    model.FromTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, model.TStartTime.Hours, model.TStartTime.Minutes, model.TStartTime.Days);
                    model.ToTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, model.TEndTime.Hours, model.TEndTime.Minutes, model.TEndTime.Days);
                    //==========
                    //if (model.FromDate.Value.Date == model.ToDate.Value.Date)
                    {
                        if (model.FromTime.Value.TimeOfDay >= model.ToTime.Value.TimeOfDay)
                        {
                            ModelState.AddModelError("IsLimitedTime", CurrentUser.GetLanguageTextFromKey("From time must be less than To time"));
                        }
                    }
                }

                //# Promotion Date Type
                if (model.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                {
                    if (model.ListWeekDayV2.Count(x => x.Status == (byte)Commons.EStatus.Deleted) == 7)
                        ModelState.AddModelError("RepeatType", CurrentUser.GetLanguageTextFromKey("Please select specific days in a week"));
                    else
                        model.DateOfWeek = string.Join("-", model.ListWeekDayV2.Where(x => x.Status != (byte)Commons.EStatus.Deleted).Select(x => x.Index).ToList());
                }
                else if (model.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                {
                    if (model.ListMonthDayV2.Count(x => x.Status == (byte)Commons.EStatus.Deleted) == 31)
                        ModelState.AddModelError("RepeatType", CurrentUser.GetLanguageTextFromKey("Please select specific days in a month"));
                    else
                        model.DateOfMonth = string.Join("-", model.ListMonthDayV2.Where(x => x.Status != (byte)Commons.EStatus.Deleted).Select(x => x.Index).ToList());
                }

                //# Spending
                if (model.ListSpendingRule != null)
                {
                    model.ListSpendingRule = model.ListSpendingRule.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                    if (model.ListSpendingRule.Count > 1)
                        model.isSpendOperatorAnd = CurrentUser.GetLanguageTextFromKey(model.ListSpendingRule.LastOrDefault().Condition).Equals(CurrentUser.GetLanguageTextFromKey("AND")) ? true : false;

                    int index = 1;
                    foreach (var item in model.ListSpendingRule)
                    {
                        if (item.SpendOnType == (byte)Commons.ESpendOnType.SpecificItem)
                        {
                            if (item.ListProduct.Count == 0)
                            {
                                ModelState.AddModelError("Spending", CurrentUser.GetLanguageTextFromKey("Please select specific items in item detail for spending rule no.") + index);
                                break;
                            }
                        }
                        if (item.Amount < 0)
                        {
                            ModelState.AddModelError("Spending", CurrentUser.GetLanguageTextFromKey("Please enter value of Quantity/Amount for spending rule no.") + index + "");
                            break;
                        }
                        else
                        {
                            item.Amount = Math.Round(item.Amount, 2);
                        }
                        index++;
                    }
                }
                else
                    ModelState.AddModelError("Spending", CurrentUser.GetLanguageTextFromKey("The Promotion must have at least one spending rule"));

                //# Earning
                if (model.ListEarningRule != null)
                {
                    model.ListEarningRule = model.ListEarningRule.Where(x => x.Status != (byte)Commons.EStatus.Deleted).ToList();
                    if (model.ListEarningRule.Count > 1)
                        model.isEarnOperatorAnd = CurrentUser.GetLanguageTextFromKey(model.ListEarningRule.LastOrDefault().Condition).Equals(CurrentUser.GetLanguageTextFromKey("AND")) ? true : false;
                    int index = 1;
                    foreach (var item in model.ListEarningRule)
                    {
                        if (item.EarnType == (byte)Commons.EEarnType.SpecificItem)
                        {
                            if (item.ListProduct.Count == 0)
                            {
                                ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("Please select specific items in item detail for earning rule no.") + index);
                                break;
                            }
                        }
                        //=======
                        if (item.bDiscountType == (byte)Commons.EValueType.Percent)
                        {
                            if (item.DiscountValue < 0 || item.DiscountValue > 100)
                            {
                                ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("Discount Percent could not larger than 100%"));
                                break;
                            }
                        }
                        else
                        {
                            if (item.DiscountValue < 0)
                            {
                                ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("Please enter value of Percent/Value for earning rule no.") + index + "");
                                break;
                            }
                            else
                            {
                                item.DiscountValue = Math.Round(item.DiscountValue, 2);
                            }
                        }
                        if (item.Quantity < 0)
                        {
                            ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("Please enter value of Quantity for earning rule no.") + index + "");
                            break;
                        }
                        //=====
                        item.DiscountType = item.bDiscountType == (byte)Commons.EValueType.Percent ? false : true;
                        index++;
                    }
                }
                else
                    ModelState.AddModelError("Earning", CurrentUser.GetLanguageTextFromKey("The Promotion must have at least one earnin rule"));

                if (!ModelState.IsValid)
                {
                    if ((ModelState["PictureUpload"]) != null && (ModelState["PictureUpload"]).Errors.Count > 0)
                        model.ImageURL = "";
                    //Return Data
                    model = GetDetail(model.ID);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //====================
                string msg = "";
                var result = _factory.InsertOrUpdatePromotion(model, ref msg);
                if (result)
                {
                    //Save Image on Server
                    if (!string.IsNullOrEmpty(model.ImageURL) && model.PictureByte != null)
                    {
                        var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", Server.MapPath(@"\")));
                        var path = string.Format("{0}{1}", originalDirectory, model.ImageURL);
                        MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                        ms.Write(photoByte, 0, photoByte.Length);
                        System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                        ImageHelper.Me.SaveCroppedImage(imageTmp, path, model.ImageURL, ref photoByte);
                        model.PictureByte = photoByte;

                        FTP.Upload(model.ImageURL, model.PictureByte);

                        ImageHelper.Me.TryDeleteImageUpdated(path);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    //return PartialView("_Edit", model);
                    ModelState.AddModelError("Name", msg);
                    return PartialView("_Edit", model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Promotion_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        [HttpGet]
        public PartialViewResult Delete(string id)
        {
            PromotionModels model = GetDetail(id);
            return PartialView("_Delete", model);
        }

        [HttpPost]
        public ActionResult Delete(PromotionModels model)
        {
            try
            {
                string msg = "";
                var result = _factory.DeletePromotion(model.ID, ref msg);
                if (!result)
                {
                    ModelState.AddModelError("Name", msg);
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Delete", model);
                }
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Promotion_Delete: " + ex);
                ModelState.AddModelError("Name", CurrentUser.GetLanguageTextFromKey("An error has occurred when deleting"));
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Delete", model);
            }
        }

        public ActionResult AddSpending(int currentOffset, string Condition)
        {
            SpendingRuleDTO group = new SpendingRuleDTO();
            group.OffSet = currentOffset;
            group.Condition = Condition;
            return PartialView("_TabSpending", group);
        }

        public ActionResult LoadItems(string StoreID, int ItemType)
        {
            var lstDish = _factoryProduct.GetListProduct(StoreID, ItemType, CurrentUser.ListOrganizationId);
            SpendingRuleDTO model = new SpendingRuleDTO();
            if (lstDish != null)
            {
                model.ListProduct = new List<PromotionProductDTO>();
                foreach (var item in lstDish)
                {
                    PromotionProductDTO product = new PromotionProductDTO()
                    {
                        ProductID = item.ID,
                        Name = item.Name,
                        ProductCode = item.ProductCode,
                        ItemType = (byte)Commons.EProductType.Dish,
                        IsAllowDiscount = item.IsAllowedDiscount
                    };
                    model.ListProduct.Add(product);
                }
            }
            return PartialView("_TableChooseItems", model);
        }

        public ActionResult AddItems(/*string[] productIDs, int currentGroupOffSet, int currentItemOffset*/ SpendingRuleDTO data)
        {
            SpendingRuleDTO model = new SpendingRuleDTO();
            if (data.ListProduct != null && data.ListProduct.Count() > 0)
                model.ListProduct = new List<PromotionProductDTO>();

            for (int i = 0; i < data.ListProduct.Count(); i++)
            {
                PromotionProductDTO item = new PromotionProductDTO();

                item.OffSet = data.currentItemOffset;

                item.ProductID = data.ListProduct[i].ProductID;
                item.ProductCode = data.ListProduct[i].ProductCode;
                item.Name = data.ListProduct[i].Name;
                item.ItemType = data.ListProduct[i].ItemType;
                item.IsAllowDiscount = data.ListProduct[i].IsAllowDiscount;

                model.ListProduct.Add(item);

                data.currentItemOffset++;
            }
            model.OffSet = data.currentgroupOffSet;

            return PartialView("_ItemModal", model);
        }

        public ActionResult AddEarning(int currentOffset, string Condition)
        {
            EarningRuleDTO group = new EarningRuleDTO();
            group.OffSet = currentOffset;
            group.Condition = Condition;
            return PartialView("_TabEarning", group);
        }
        public ActionResult LoadItemsEarn(string StoreID, int ItemType)
        {
            var lstDish = _factoryProduct.GetListProduct(StoreID, ItemType, CurrentUser.ListOrganizationId);
            EarningRuleDTO model = new EarningRuleDTO();
            if (lstDish != null)
            {
                lstDish = lstDish.Where(ww => ww.IsAllowedDiscount).ToList();
                model.ListProduct = new List<PromotionProductDTO>();
                foreach (var item in lstDish)
                {
                    PromotionProductDTO product = new PromotionProductDTO()
                    {
                        ProductID = item.ID,
                        Name = item.Name,
                        ProductCode = item.ProductCode,
                        ItemType = (byte)Commons.EProductType.Dish
                    };
                    model.ListProduct.Add(product);
                }
            }
            return PartialView("_TableChooseItemsEarn", model);
        }
        public ActionResult AddItemsEarn(/*string[] productIDs, int currentGroupOffSet, int currentItemOffset*/ EarningRuleDTO data)
        {
            EarningRuleDTO model = new EarningRuleDTO();
            if (data.ListProduct != null && data.ListProduct.Count() > 0)
                model.ListProduct = new List<PromotionProductDTO>();

            for (int i = 0; i < data.ListProduct.Count(); i++)
            {
                PromotionProductDTO item = new PromotionProductDTO();

                item.OffSet = data.currentItemOffset;

                item.ProductID = data.ListProduct[i].ProductID;
                item.ProductCode = data.ListProduct[i].ProductCode;
                item.Name = data.ListProduct[i].Name;
                item.ItemType = data.ListProduct[i].ItemType;
                model.ListProduct.Add(item);

                data.currentItemOffset++;
            }
            model.OffSet = data.currentgroupOffSet;
            return PartialView("_ItemModalEarn", model);
        }

        public ActionResult Import()
        {
            SandboxImportModel model = new SandboxImportModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Import(SandboxImportModel model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }
                //if (model.ImageZipUpload == null || model.ImageZipUpload.ContentLength <= 0)
                //{
                //    ModelState.AddModelError("ImageZipUpload", "Image Folder (.zip) cannot be null");
                //    return View(model);
                //}
                if (model.ImageZipUpload != null)
                {
                    if (!Path.GetExtension(model.ImageZipUpload.FileName).ToLower().Equals(".zip"))
                    {
                        ModelState.AddModelError("ImageZipUpload", "");
                        return View(model);
                    }
                }
                if (model.ExcelUpload == null || model.ExcelUpload.ContentLength <= 0)
                {
                    ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey("Excel filename cannot be null"));
                    return View(model);
                }

                FileInfo[] listFiles = new FileInfo[] { };
                string serverZipExtractPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads") + "/ExtractFolder";
                if (model.ImageZipUpload != null && model.ImageZipUpload.ContentLength > 0)
                {
                    bool isFolderEmpty;
                    string fileName = Path.GetFileName(model.ImageZipUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);

                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ImageZipUpload.SaveAs(filePath);

                    //extract file
                    CommonHelper.ExtractZipFile(filePath, serverZipExtractPath);

                    //delete zip file after extract
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    isFolderEmpty = CommonHelper.IsDirectoryEmpty(serverZipExtractPath);

                    if (!isFolderEmpty)
                    {
                        string[] extensions = new[] { ".jpg", ".png", ".jpeg" };
                        DirectoryInfo dInfo = new DirectoryInfo(serverZipExtractPath);
                        //Getting Text files
                        listFiles = dInfo.EnumerateFiles()
                                 .Where(f => extensions.Contains(f.Extension.ToLower()))
                                 .ToArray();
                    }
                }

                PromotionImportResultModels importModel = new PromotionImportResultModels();
                string msg = "";
                // read excel file
                if (model.ExcelUpload != null && model.ExcelUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(model.ExcelUpload.FileName);
                    string filePath = string.Format("{0}/{1}", System.Web.HttpContext.Current.Server.MapPath("~/Uploads"), fileName);

                    //upload file to server
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    model.ExcelUpload.SaveAs(filePath);

                    int totalRowExcel;
                    importModel.ListImport = _factory.Import(filePath, listFiles, out totalRowExcel, model.ListStores, ref msg);
                    importModel.TotalRowExcel = totalRowExcel;

                    //delete folder extract after get file.
                    if (Directory.Exists(serverZipExtractPath))
                        Directory.Delete(serverZipExtractPath, true);
                    //delete file excel after insert to database
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                if (msg.Equals(""))
                {
                    return View("ImportDetail", importModel);
                }
                else
                {
                    _logger.Error("Promotion_Import: " + msg);
                    ModelState.AddModelError("ImageZipUpload", msg);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Promotion_Import: " + e);
                ModelState.AddModelError("ImageZipUpload", CurrentUser.GetLanguageTextFromKey("Import file have error"));
                return View(model);
            }
        }

        public ActionResult Export()
        {
            PromotionModels model = new PromotionModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(PromotionModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store."));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsPromotions = wb.Worksheets.Add("Promotions");
                var wsSpending = wb.Worksheets.Add("Spending");
                var wsSpendingProduct = wb.Worksheets.Add("SpendingProduct");
                var wsEarning = wb.Worksheets.Add("Earning");
                var wsEarningProduct = wb.Worksheets.Add("EarningProduct");

                StatusResponse response = _factory.Export(ref wsPromotions, ref wsSpending, ref wsSpendingProduct, ref wsEarning, ref wsEarningProduct, model.ListStores);

                if (!response.Status)
                {
                    ModelState.AddModelError("", response.MsgError);
                    return View(model);
                }

                ViewBag.wb = wb;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Charset = UTF8Encoding.UTF8.WebName;
                Response.ContentEncoding = UTF8Encoding.UTF8;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", CommonHelper.GetExportFileName("Promotions").Replace(" ", "_")));

                using (var memoryStream = new MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(HttpContext.Response.OutputStream);
                    memoryStream.Close();
                }
                HttpContext.Response.End();
                return RedirectToAction("Export");
            }
            catch (Exception e)
            {
                _logger.Error("Promotion_Export: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }
    }
}