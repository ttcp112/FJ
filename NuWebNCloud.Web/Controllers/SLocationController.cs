using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory.Settings;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Models.Settings.Location;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SLocationController : HQController
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private GeneralSettingFactory _Genfactory = null;
        private LocationFactory _factory = null;

        public SLocationController()
        {
            _Genfactory = new GeneralSettingFactory();
            _factory = new LocationFactory();
            ViewBag.ListStore = GetListStore();
        }

        // GET: SLocation
        public ActionResult Index()
        {
            try
            {
                GeneralSettingViewModels model = new GeneralSettingViewModels();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Location_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(GeneralSettingViewModels model)
        {
            try
            {
                var listStore = ViewBag.ListStore;
                foreach (var item in listStore)
                {
                    model.ListItem.Add(new StoreModels
                    {
                        Name = item.Text,
                        Id = item.Value
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("GeneralSetting_Search : " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }


        public LocationModels GetDetail(string StoreID)
        {
            try
            {
                LocationModels model = new LocationModels();
                model.StoreID = StoreID;
                string StoreName = "";
                var listSettings = _Genfactory.GetListGeneralSetting(ref StoreName, StoreID, null);
                model.StoreName = StoreName;
                if (listSettings.Count != 0)
                {
                    var Region = listSettings.Where(x => x.Code == (byte)Commons.ESetting.Region).FirstOrDefault().Value;
                    if (!string.IsNullOrEmpty(Region))
                    {
                        var listSettingRegion = _factory.GetRegion(StoreID, Region);
                        if (listSettingRegion.Count != 0)
                        {
                            var IsPrintRoundingAmount = listSettingRegion.Where(x => x.Code == (byte)Commons.ESetting.PrintRoundingAmount).FirstOrDefault().Value;
                            model.IsPrintRoundingAmount = bool.Parse(IsPrintRoundingAmount);

                            var IsPrintTaxCode = listSettingRegion.Where(x => x.Code == (byte)Commons.ESetting.PrintTaxCode).FirstOrDefault().Value;
                            model.IsPrintTaxCode = bool.Parse(IsPrintTaxCode);

                            var IsPrintSummaryTax = listSettingRegion.Where(x => x.Code == (byte)Commons.ESetting.PrintSummaryTax).FirstOrDefault().Value;
                            model.IsPrintSummaryTax = bool.Parse(IsPrintSummaryTax);

                            var IsPrintCustomerClaimTax = listSettingRegion.Where(x => x.Code == (byte)Commons.ESetting.PrintCustomerClaimTax).FirstOrDefault().Value;
                            model.IsPrintCustomerClaimTax = bool.Parse(IsPrintCustomerClaimTax);
                        }
                    }
                    //=========
                    var listCountry = _factory.GetListCountry();
                    foreach (var item in listCountry)
                    {
                        model.ListCountry.Add(new SelectListItem
                        {
                            Text = item.Name,
                            Value = item.Alpha2Code
                        });
                    }
                    var objRegion = listCountry.Where(x => x.Alpha2Code.Equals(Region)).FirstOrDefault();
                    model.RegionName = objRegion == null ? "" : objRegion.Name;
                    model.Region = Region;
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("Location_Detail : " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string StoreID)
        {
            LocationModels model = GetDetail(StoreID);
            return PartialView("_View", model);
        }

        public PartialViewResult Edit(string StoreID)
        {
            LocationModels model = GetDetail(StoreID);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(LocationModels model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.StoreID))
                    ModelState.AddModelError("StoreID", CurrentUser.GetLanguageTextFromKey("Please choose store"));

                if (string.IsNullOrEmpty(model.Region))
                    ModelState.AddModelError("Region", CurrentUser.GetLanguageTextFromKey("Please choose region"));

                List<SettingDTO> ListSettings = new List<SettingDTO>();
                ListSettings.Add(new SettingDTO
                {
                    SettingId = Commons.ESetting.Region.ToString("d"),
                    Value = model.Region.ToString()
                });
                // Malaysia, there are some settings:
                if (model.Region.ToLower().Equals("my"))
                {
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.PrintRoundingAmount.ToString("d"),
                        Value = model.IsPrintRoundingAmount.ToString().ToLower()
                    });
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.PrintTaxCode.ToString("d"),
                        Value = model.IsPrintTaxCode.ToString().ToLower()
                    });
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.PrintSummaryTax.ToString("d"),
                        Value = model.IsPrintSummaryTax.ToString().ToLower()
                    });
                    ListSettings.Add(new SettingDTO
                    {
                        SettingId = Commons.ESetting.PrintCustomerClaimTax.ToString("d"),
                        Value = model.IsPrintCustomerClaimTax.ToString().ToLower()
                    });
                }
                //==============
                GeneralSettingModels objSetting = new GeneralSettingModels();
                objSetting.ListSettings = ListSettings;
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                //====================
                objSetting.StoreID = model.StoreID;
                var result = _Genfactory.InsertOrUpdateGeneralSetting(objSetting);
                if (result)
                    return RedirectToAction("Index");
                else
                    return PartialView("_Edit", model);
            }
            catch (Exception ex)
            {
                _logger.Error("Location_Edit: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }
    }
}