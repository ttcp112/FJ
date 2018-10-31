using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Integration.Factory.Sandbox.Categories;
using NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Models.Settings.Season;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory.Product;

namespace NuWebNCloud.Web.Controllers
{
    public class SBInventoryBaseController : HQController
    {
        SeasonFactory _seasonFactory = null;
        CategoriesFactory _categoryFactory = null;
        InteCategoriesFactory _categoryFactoryInte = null;

        PrinterFactory _printerFactory = null;

        public SBInventoryBaseController()
        {
            _seasonFactory = new SeasonFactory();
            _categoryFactory = new CategoriesFactory();
            _printerFactory = new PrinterFactory();
            _categoryFactoryInte = new InteCategoriesFactory();
        }

        /*Price - Season*/
        public List<PriceItem> GetPrices(List<PriceItem> lstPrices, string StoreID)
        {
            List<SeasonModels> lstItem = _seasonFactory.GetListSeason(StoreID, null, CurrentUser.ListOrganizationId);
            if (lstItem != null)
            {
                List<SelectListItem> lstSlcSeason = new List<SelectListItem>();
                foreach (SeasonModels season in lstItem)
                    lstSlcSeason.Add(new SelectListItem
                    {
                        Text = season.Name,// + " [" + season.StoreName + "]",
                        Value = season.ID
                    });

                if (lstPrices != null)
                    foreach (PriceItem price in lstPrices)
                        price.ListSeasons = lstSlcSeason;
            }
            return lstPrices;
        }

        //public ActionResult LoadSeason(string[] StoreID)
        //{
        //    List<SeasonModels> lstItem = null;
        //    StoreID = StoreID.Where(ww => !string.IsNullOrEmpty(ww)).ToArray();
        //    if (StoreID.Length == 0)
        //    {
        //        lstItem = new List<SeasonModels>();
        //    }
        //    else
        //    {
        //        lstItem = new List<SeasonModels>();
        //        foreach (var item in StoreID)
        //        {
        //            var result = _seasonFactory.GetListSeason(item, null, CurrentUser.ListOrganizationId);
        //            lstItem.AddRange(result);
        //        }
        //    }
        //    return Json(lstItem, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult LoadSeason(string StoreID)
        {
            List<SeasonModels> lstItem = _seasonFactory.GetListSeason(StoreID, null, CurrentUser.ListOrganizationId);
            return Json(lstItem, JsonRequestBehavior.AllowGet);
        }

        /*Category*/
        public List<SelectListItem> GetSelectListCategories(string StoreID, string itemType)
        {
            List<CategoriesModels> lstData = _categoryFactory.GetListCategory(StoreID, null, itemType, CurrentUser.ListOrganizationId);
            List<SelectListItem> slcCate = new List<SelectListItem>();
            if (lstData != null)
            {
                foreach (CategoriesModels cate in lstData)
                {
                    if (cate.ListChild != null)
                    {
                        foreach (var item in cate.ListChild)
                        {
                            slcCate.Add(new SelectListItem
                            {
                                Text = item.Name,// + " [" + item.StoreName + "]",
                                Value = item.ID
                            });
                        }
                    }
                    else
                    {
                        slcCate.Add(new SelectListItem
                        {
                            Text = cate.Name + " [" + cate.StoreName + "]",
                            Value = cate.ID
                        });
                    }
                }
            }
            return slcCate;
        }

        // Updated 08282017
        public List<SBInventoryBaseCateGroupViewModel> GetSelectListCategoriesSortParent(string StoreID, string itemType)
        {
            List<SBInventoryBaseCateGroupViewModel> lstCateGroup = new List<SBInventoryBaseCateGroupViewModel>();
            if (!string.IsNullOrEmpty(StoreID))
            {
                lstCateGroup = _categoryFactory.GetListCategorySortParent(StoreID, null, itemType, CurrentUser.ListOrganizationId);
            }
            return lstCateGroup;
        }

        public List<SelectListItem> GetSelectListCategoriesInte(string StoreID, string itemType)
        {
            List<string> lstStore = new List<string>();
            lstStore.Add(StoreID);

            List<InteCategoriesModels> lstData = _categoryFactoryInte.GetListCategory(lstStore, null, itemType, CurrentUser.ListOrganizationId);
            List<SelectListItem> slcCate = new List<SelectListItem>();
            if (lstData != null)
            {
                foreach (InteCategoriesModels cate in lstData)
                {
                    if (cate.ListChild != null)
                    {
                        foreach (var item in cate.ListChild)
                        {
                            slcCate.Add(new SelectListItem
                            {
                                Text = item.Name,// + " [" + item.StoreName + "]",
                                Value = item.ID
                            });
                        }
                    }
                    else
                    {
                        slcCate.Add(new SelectListItem
                        {
                            Text = cate.Name,// + " [" + cate.StoreName + "]",
                            Value = cate.ID
                        });
                    }
                }
            }
            return slcCate;
        }

        // Updated 08302017
        public List<SBInventoryBaseCateGroupViewModel> GetSelectListCategoriesInteSortParent(string StoreID, string itemType)
        {
            List<string> lstStore = new List<string>();
            lstStore.Add(StoreID);

            List<SBInventoryBaseCateGroupViewModel> lstCateGroup = _categoryFactoryInte.GetListCategorySortParent(lstStore, null, itemType, CurrentUser.ListOrganizationId);
            return lstCateGroup;
        }

        //public ActionResult LoadCategory(string[] StoreID, string itemType, string cateID = "0")
        //{
        //    List<CategoriesModels> lstData = null;
        //    SBInventoryBaseModel model = new SBInventoryBaseModel();
        //    StoreID =  StoreID.Where(ww => !string.IsNullOrEmpty(ww)).ToArray();
        //    if (StoreID.Length == 0)
        //    {
        //        model.ListCategories.Add(new SelectListItem());
        //    }
        //    else
        //    {
        //        lstData = new List<CategoriesModels>();
        //        foreach (var item in StoreID)
        //        {
        //            var temp = _categoryFactory.GetListCategory(item, null, itemType, CurrentUser.ListOrganizationId);
        //            lstData.AddRange(temp);
        //        }

        //        //SBInventoryBaseModel model = new SBInventoryBaseModel();
        //        if (lstData != null && lstData.Count > 0)
        //        {
        //            foreach (CategoriesModels cate in lstData)
        //            {
        //                if (cate.ListChild != null)
        //                {
        //                    foreach (var item in cate.ListChild)
        //                    {
        //                        model.ListCategories.Add(new SelectListItem
        //                        {
        //                            Value = item.ID,
        //                            Text = item.Name + " [" + item.StoreName + "]",
        //                            Selected = item.ID.Equals(cateID) ? true : false
        //                        });
        //                    }
        //                }
        //                else
        //                {
        //                    model.ListCategories.Add(new SelectListItem
        //                    {
        //                        Value = cate.ID,
        //                        Text = cate.Name + " [" + cate.StoreName + "]",
        //                        Selected = cate.ID.Equals(cateID) ? true : false
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return PartialView("~/Views/SBInventoryBase/_DropDownListCategory.cshtml", model);
        //}

        //public ActionResult LoadCategory(string StoreID, string itemType, string cateID = "0")
        //{
        //    List<CategoriesModels> lstData = _categoryFactory.GetListCategory(StoreID, null, itemType, CurrentUser.ListOrganizationId);
        //    SBInventoryBaseModel model = new SBInventoryBaseModel();
        //    if (lstData != null && lstData.Count > 0)
        //        foreach (CategoriesModels cate in lstData)
        //        {
        //            if (cate.ListChild != null)
        //            {
        //                foreach (var item in cate.ListChild)
        //                {
        //                    model.ListCategories.Add(new SelectListItem
        //                    {
        //                        Value = item.ID,
        //                        Text = item.Name,// + " [" + item.StoreName + "]",
        //                        Selected = item.ID.Equals(cateID) ? true : false
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                model.ListCategories.Add(new SelectListItem
        //                {
        //                    Value = cate.ID,
        //                    Text = cate.Name + " [" + cate.StoreName + "]",
        //                    Selected = cate.ID.Equals(cateID) ? true : false
        //                });
        //            }
        //        }
        //    return PartialView("~/Views/SBInventoryBase/_DropDownListCategory.cshtml", model);
        //}

        // Updated 08282017
        public ActionResult LoadCategory(string StoreID, string itemType, string cateID = "0")
        {
            List<SBInventoryBaseCateGroupViewModel> lstCateGroup = _categoryFactory.GetListCategorySortParent(StoreID, null, itemType, CurrentUser.ListOrganizationId, cateID);

            SBInventoryBaseModel model = new SBInventoryBaseModel();
            model.lstCateGroup = lstCateGroup;

            return PartialView("~/Views/SBInventoryBase/_DropDownListCategory.cshtml", model);
        }

        /*Printer*/
        public List<PrinterModels> GetSelectListPrinters(string StoreID, List<PrinterOnProductModels> ListPrinter)
        {
            List<PrinterModels> lstData = _printerFactory.GetListPrinter(StoreID, null, CurrentUser.ListOrganizationId);
            List<PrinterModels> lstResult = new List<PrinterModels>();
            //if (lstData != null)
            //{
            //    foreach (PrinterModels item in lstData)
            //        lstResult.Add(new PrinterModels
            //        {
            //            PrinterName = item.PrinterName,// + " [" + item.StoreName + "]",
            //            Id = item.Id,
            //            Status = 9
            //        });
            //}
            //lstResult.ForEach(x =>
            //{
            //    if (ListPrinter.Count != 0)
            //    {
            //        x.Status = ListPrinter.Where(z => z.PrinterID.Equals(x.Id)).FirstOrDefault() != null ? (byte)Commons.EStatus.Actived : (byte)Commons.EStatus.Deleted;
            //    }
            //});
            
            if (lstData != null && lstData.Any())
            {
                if (ListPrinter == null)
                {
                    ListPrinter = new List<PrinterOnProductModels>();
                }
                var printer = new PrinterOnProductModels();
                bool isMapProduct = false;
                int status = (int)Commons.EStatus.Deleted;
                foreach (PrinterModels item in lstData)
                {
                    isMapProduct = false;
                    status = (int)Commons.EStatus.Deleted;
                    printer = ListPrinter.Where(w => w.PrinterID == item.Id).FirstOrDefault();
                    if (printer != null)
                    {
                        isMapProduct = true;
                        status = printer.IsActive ? (int)Commons.EStatus.Actived : (int)Commons.EStatus.Deleted;
                    }
                    lstResult.Add(new PrinterModels()
                    {
                        PrinterName = item.PrinterName,// + " [" + item.StoreName + "]",
                        Id = item.Id,
                        Status = status,
                        IsMapProduct = isMapProduct
                    });
                }
            }
            lstResult = lstResult.OrderBy(o => o.PrinterName).ToList();
            return lstResult;
        }

        //public ActionResult LoadPrinter(string[] StoreID)
        //{
        //    List<PrinterModels> lstData = null;
        //    StoreID = StoreID.Where(ww => !string.IsNullOrEmpty(ww)).ToArray();
        //    if (StoreID.Length == 0)
        //    {
        //        lstData = new List<PrinterModels>();
        //    }
        //    else
        //    {
        //        lstData = new List<PrinterModels>();
        //        foreach (var item in StoreID)
        //        {
        //            var temp = _printerFactory.GetListPrinter(item, null, CurrentUser.ListOrganizationId);
        //            lstData.AddRange(temp);
        //        }
        //    }            
           
        //    ProductModels model = new ProductModels();
        //    if (lstData != null && lstData.Count > 0)
        //        foreach (PrinterModels data in lstData)
        //            model.LstPrinter.Add(new PrinterModels
        //            {
        //                Id = data.Id,
        //                PrinterName = data.PrinterName + " [" + data.StoreName + "]",
        //                Status = 9
        //            });
        //    return PartialView("~/Views/SBInventoryBase/_ChoosePrinter.cshtml", model);
        //}

        public ActionResult LoadPrinter(string StoreID)
        {
            List<PrinterModels> lstData = _printerFactory.GetListPrinter(StoreID, null, CurrentUser.ListOrganizationId);
            ProductModels model = new ProductModels();
            if (lstData != null && lstData.Count > 0)
                foreach (PrinterModels data in lstData)
                    model.LstPrinter.Add(new PrinterModels
                    {
                        Id = data.Id,
                        PrinterName = data.PrinterName,// + " [" + data.StoreName + "]",
                        Status = 9
                    });
            return PartialView("~/Views/SBInventoryBase/_ChoosePrinter.cshtml", model);
        }

        //public ActionResult LoadTimeSlot(string[] StoreID)
        //{
        //    ProductModels model = new ProductModels();
        //    List<SeasonModels> lstItem = null;
        //    StoreID = StoreID.Where(ww => !string.IsNullOrEmpty(ww)).ToArray();
        //    if (StoreID.Length == 0)
        //    {
        //        lstItem = new List<SeasonModels>();
        //    }
        //    else
        //    {
        //        foreach(var item in StoreID)
        //        {
        //            lstItem = new List<SeasonModels>();
        //            var result = _seasonFactory.GetListSeason(item, null, CurrentUser.ListOrganizationId);
        //            lstItem.AddRange(result);
        //        }
        //    }
        //    if (lstItem != null && lstItem.Count > 0)
        //        foreach (SeasonModels ss in lstItem)
        //        {
        //            model.ListSeason.Add(new SeasonModels
        //            {
        //                ID = ss.ID,
        //                Name = ss.Name,
        //                RepeatType = ss.RepeatType,
        //                ListDay = ss.ListDay,

        //                StartDate = ss.StartDate,
        //                EndDate = ss.EndDate,

        //                StartTime = ss.StartTime.Value.ToLocalTime(),
        //                EndTime = ss.EndTime.Value.ToLocalTime(),

        //                Status = (byte)Commons.EStatus.Deleted,

        //                Unlimited = (ss.StartTime.Value.Date == Commons._UnlimitedDate
        //                        || ss.EndTime.Value.Date == Commons._UnlimitedDate) ? true : false
        //            });
        //        }
        //    model.ListSeason.ForEach(x =>
        //    {
        //        if (x.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
        //        {
        //            x.ListWeekDayV2.ForEach(z =>
        //            {
        //                if (x.ListDay.Contains(z.Index))
        //                {
        //                    z.IsActive = true;
        //                    z.Status = 1;
        //                }
        //            });
        //        }
        //        else if (x.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
        //        {
        //            x.ListMonthDayV2.ForEach(z =>
        //            {
        //                if (x.ListDay.Contains(z.Index))
        //                {
        //                    z.IsActive = true;
        //                    z.Status = 1;
        //                }
        //            });
        //        }
        //    });
        //    return PartialView("~/Views/SBInventoryBase/_ChooseSeason.cshtml", model);
        //}

        public ActionResult LoadTimeSlot(string StoreID)
        {
            ProductModels model = new ProductModels();
            List<SeasonModels> lstItem = _seasonFactory.GetListSeason(StoreID, null, CurrentUser.ListOrganizationId);
            if (lstItem != null && lstItem.Count > 0)
                foreach (SeasonModels ss in lstItem)
                {
                    model.ListSeason.Add(new SeasonModels
                    {
                        ID = ss.ID,
                        Name = ss.Name,
                        RepeatType = ss.RepeatType,
                        ListDay = ss.ListDay,

                        StartDate = ss.StartDate,
                        EndDate = ss.EndDate,

                        StartTime = ss.StartTime.Value.ToLocalTime(),
                        EndTime = ss.EndTime.Value.ToLocalTime(),

                        Status = (byte)Commons.EStatus.Deleted,

                        Unlimited = (ss.StartTime.Value.Date == Commons._UnlimitedDate
                                || ss.EndTime.Value.Date == Commons._UnlimitedDate) ? true : false
                    });
                }
            model.ListSeason.ForEach(x =>
            {
                if (x.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                {
                    x.ListWeekDayV2.ForEach(z =>
                    {
                        if (x.ListDay.Contains(z.Index))
                        {
                            z.IsActive = true;
                            z.Status = 1;
                        }
                    });
                }
                else if (x.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                {
                    x.ListMonthDayV2.ForEach(z =>
                    {
                        if (x.ListDay.Contains(z.Index))
                        {
                            z.IsActive = true;
                            z.Status = 1;
                        }
                    });
                }
            });
            return PartialView("~/Views/SBInventoryBase/_ChooseSeason.cshtml", model);
        }

        // Load POS
        public ActionResult LoadTimeSlotPOS(string StoreID)
        {
            ProductModels model = new ProductModels();
            List<SeasonModels> lstItem = _seasonFactory.GetListSeason(StoreID, null, CurrentUser.ListOrganizationId);
            if (lstItem != null && lstItem.Count > 0)
                foreach (SeasonModels ss in lstItem)
                {
                    model.ListSeasonPOS.Add(new SeasonModels
                    {
                        ID = ss.ID,
                        Name = ss.Name,
                        RepeatType = ss.RepeatType,
                        ListDay = ss.ListDay,

                        StartDate = ss.StartDate,
                        EndDate = ss.EndDate,

                        StartTime = ss.StartTime.Value.ToLocalTime(),
                        EndTime = ss.EndTime.Value.ToLocalTime(),

                        Status = (byte)Commons.EStatus.Deleted,

                        Unlimited = (ss.StartTime.Value.Date == Commons._UnlimitedDate
                                || ss.EndTime.Value.Date == Commons._UnlimitedDate) ? true : false,

                        IsPOS = true
                    });
                }
            model.ListSeasonPOS.ForEach(x =>
            {
                if (x.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                {
                    x.ListWeekDayV2.ForEach(z =>
                    {
                        if (x.ListDay.Contains(z.Index))
                        {
                            z.IsActive = true;
                            z.Status = 1;
                        }
                    });
                }
                else if (x.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                {
                    x.ListMonthDayV2.ForEach(z =>
                    {
                        if (x.ListDay.Contains(z.Index))
                        {
                            z.IsActive = true;
                            z.Status = 1;
                        }
                    });
                }
            });
            return PartialView("~/Views/SBInventoryBase/_ChooseSeasonPOS.cshtml", model);
        }

        public List<SeasonModels> GetListTimeSlot02(string StoreID, List<ProductSeasonDTO> ListProductSeason, bool IsPOS = false)
        {
            List<SeasonModels> lstItem = _seasonFactory.GetListSeason(StoreID, null, CurrentUser.ListOrganizationId);
            List<SeasonModels> lstResult = new List<SeasonModels>();
            if (lstItem != null && lstItem.Count > 0)
                foreach (SeasonModels ss in lstItem)
                {
                    lstResult.Add(new SeasonModels
                    {
                        ID = ss.ID,
                        Name = ss.Name,
                        RepeatType = ss.RepeatType,
                        ListDay = ss.ListDay,

                        StartDate = ss.StartDate,
                        EndDate = ss.EndDate,

                        StartTime = ss.StartTime.Value.ToLocalTime(),
                        EndTime = ss.EndTime.Value.ToLocalTime(),

                        Status = (byte)Commons.EStatus.Deleted,

                        Unlimited = (ss.StartTime.Value.Date == Commons._UnlimitedDate
                                    || ss.EndTime.Value.Date == Commons._UnlimitedDate) ? true : false
                    });
                }
            lstResult.ForEach(x =>
            {
                if (ListProductSeason == null)
                    ListProductSeason = new List<ProductSeasonDTO>();

                if (ListProductSeason.Count != 0)
                {
                    x.Status = ListProductSeason.Where(z => z.SeasonID.Equals(x.ID)).FirstOrDefault() != null ? (byte)Commons.EStatus.Actived : (byte)Commons.EStatus.Deleted;
                }
                //=============
                if (x.RepeatType == (byte)Commons.ERepeatType.DayOfWeek)
                {
                    x.ListWeekDayV2.ForEach(z =>
                    {
                        if (x.ListDay.Contains(z.Index))
                        {
                            z.IsActive = true;
                            z.Status = 1;
                        }
                    });
                }
                else if (x.RepeatType == (byte)Commons.ERepeatType.DayOfMonth)
                {
                    x.ListMonthDayV2.ForEach(z =>
                    {
                        if (x.ListDay.Contains(z.Index))
                        {
                            z.IsActive = true;
                            z.Status = 1;
                        }
                    });
                }
            });
            return lstResult;
        }

        
        public ActionResult LoadServiceCharge(string StoreID)
        {
            TipServiceChargeFactory _factory = new TipServiceChargeFactory();
            var item = _factory.GetListTipServiceCharge(StoreID);
            return Json(item, JsonRequestBehavior.AllowGet);
        }


        // Updated 04192018, for list stores group by company
        public ActionResult LoadListStoreExtendTo(string StoreId)
        {
            // Return for new list stores extend
            InteProductViewModels model = new InteProductViewModels();
            var lstStoreView = (List<StoreModels>)ViewBag.StoreID.Items;
            var temps = lstStoreView.Where(ww => ww.Id != StoreId).ToList();
            model.ListStoreTo = new SelectList(temps, "Id", "Name", "CompanyName", 1);
            return PartialView("_LoadListStoreExtendTo", model);
        }

    }
}