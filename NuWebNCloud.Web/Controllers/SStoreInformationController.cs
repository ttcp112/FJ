using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using NuWebNCloud.Web.App_Start;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

// Updated 08292017
using NuWebNCloud.Shared.Models.Api;

namespace NuWebNCloud.Web.Controllers
{
    [NuAuth]
    public class SStoreInformationController : HQController
    {
        private StoreFactory _factory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SStoreInformationController()
        {
            _factory = new StoreFactory();
            ViewBag.ListStore = GetListStore();
        }

        public ActionResult Index()
        {
            try
            {
                StoreModelView model = new StoreModelView();
                model.StoreID = CurrentUser.StoreId;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("StoreInformations_Index: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult Search(StoreModelView model)
        {
            try
            {
                List<SelectListItem> lstStore = ViewBag.ListStore;
                if (lstStore.Count == 1)
                {
                    model.StoreID = lstStore[0].Value;
                }
                StoreGroupByCompany groupComp = new StoreGroupByCompany();
                var data = _factory.GetListStores(model.StoreID, null, CurrentUser.ListOrganizationId);
                var lstdataGroupByCompany = data.GroupBy(item => new {CompanyId = item.CompanyID , CompanyName = item.CompanyName }).ToList();
                lstdataGroupByCompany.ForEach(comp => {
                    groupComp = new StoreGroupByCompany();
                    groupComp.CompanyName = comp.Key.CompanyName;
                    groupComp.CompanyId = comp.Key.CompanyId;
                    var lsStore = comp.ToList();
                    lsStore.ForEach(item => {
                        groupComp.List_Store.Add(new SStoreModels()
                        {
                            OrganizationName = item.OrganizationName,
                            Name = item.Name,
                            IsActive = item.IsActive,
                            CompanyName = item.CompanyName,
                            IndustryName = item.IndustryName,
                            Street = item.Street,
                            Phone = item.Phone,
                            ImageURL = item.ImageURL,
                            ID = item.ID,
                            Status = item.Status,

                        });                       
                    });
                    groupComp.List_Store = groupComp.List_Store.OrderBy(z => z.Name).ToList();
                    model.List_Store_Group.Add(groupComp);
                });
                
            }
            catch (Exception ex)
            {
                _logger.Error("StoreInformations_Search: " + ex);
                return new HttpStatusCodeResult(400, ex.Message);
            }
            return PartialView("_ListData", model);
        }

        public SStoreModels GetDetail(string id)
        {
            try
            {
                SStoreModels model = _factory.GetListStores(null, id)[0];
                model.StoreID = id;
                model.ImageURL = string.IsNullOrEmpty(model.ImageURL) ? Commons.Image100_100 : model.ImageURL;
                //=======Working Time
                if (model.ListBusinessHour != null)
                {
                    model.ListBusinessHour = model.ListBusinessHour.OrderBy(x => x.Day).ToList();
                    model.GetTime();
                    for (int i = 0; i < model.ListBusinessHour.Count; i++)
                    {
                        var item = model.ListBusinessHour[i];
                        if (item.IsOffline)
                        {
                            item.From = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF");
                            item.To = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF");
                        }
                        else
                        {
                            item.From = item.FromTime.ToLocalTime().ToString("HH:mm");
                            item.To = item.ToTime.ToLocalTime().ToString("HH:mm");
                        }
                        item.IsOffline = !item.IsOffline;
                        switch (item.Day)
                        {
                            case 2:
                                item.StrDate = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Mon");
                                break;
                            case 3:
                                item.StrDate = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tue");
                                break;
                            case 4:
                                item.StrDate = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Wed");
                                break;
                            case 5:
                                item.StrDate = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Thu");
                                break;
                            case 6:
                                item.StrDate = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Fri");
                                break;
                            case 7:
                                item.StrDate = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sat");
                                break;
                            case 8:
                                item.StrDate = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sun");
                                break;
                        }
                    }
                }
                else
                {
                    model.GetValue();
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error("StoreInformations_ Detail: " + ex);
                return null;
            }
        }

        [HttpGet]
        public new PartialViewResult View(string id)
        {
            SStoreModels model = GetDetail(id);
            return PartialView("_View", model);
        }
        public PartialViewResult Edit(string id)
        {

            SStoreModels model = GetDetail(id);
            List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();

            foreach (var country in lstCountries)
            {
                model.ListCountries.Add(new SelectListItem
                {
                    Value = country.Name,
                    Text = country.Name,
                    Selected = country.Name.Equals(model.Country) ? true : false
                });
            }
            model.ListTimezones = GetTimezonesOfCountry(lstCountries, model.Country, model.TimeZone);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(SStoreModels model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.ImageURL))
                {
                    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");
                }

                if (model.PictureUpload != null && model.PictureUpload.ContentLength > 0)
                {
                    Byte[] imgByte = new Byte[model.PictureUpload.ContentLength];
                    model.PictureUpload.InputStream.Read(imgByte, 0, model.PictureUpload.ContentLength);
                    model.PictureByte = imgByte;
                    model.ImageURL = Guid.NewGuid() + Path.GetExtension(model.PictureUpload.FileName);
                    model.PictureUpload = null;
                }
                else
                    model.ImageURL = model.ImageURL.Replace(Commons._PublicImages, "").Replace(Commons.Image100_100, "");

                // Country
                // Updated 08292017
                string countryChoose = null;
                if (!string.IsNullOrEmpty(model.Country))
                {
                    countryChoose = model.Country;
                }

                // Updated 08312017
                string timeZoneChoose = null;
                if (!string.IsNullOrEmpty(model.TimeZone))
                {
                    timeZoneChoose = model.TimeZone;
                }

                //ListBusinessHour
                for (int i = 0; i < model.ListBusinessHour.Count; i++)
                {
                    var item = model.ListBusinessHour[i];

                    //item.IsOffline = (item.From.ToLower().Equals("off") || item.To.ToLower().Equals("off")) ? true : false;
                    //if (item.IsOffline)
                    //{
                        bool itemIsOff = (_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item.From).Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF")) 
                                    || _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item.To).Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF"))) ? false : true;
                        item.IsOffline = itemIsOff;
                    //}

                    if (/*!item.From.ToLower().Equals("off") && !item.To.ToLower().Equals("off")*/item.IsOffline)
                    {
                        item.FromTime = Convert.ToDateTime(item.From);
                        item.ToTime = Convert.ToDateTime(item.To);
                        if (item.FromTime.TimeOfDay > item.ToTime.TimeOfDay)
                        {
                            ModelState.AddModelError("ListBusinessHour", CurrentUser.GetLanguageTextFromKey("From time must be less than To time"));
                            break;
                        }
                    }
                    item.IsOffline = !item.IsOffline;
                }
                if (!ModelState.IsValid)
                {
                    model = GetDetail(model.ID);

                    // Get list Countries
                    // Updated 08292017
                    List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
                    foreach (var country in lstCountries)
                    {
                        model.ListCountries.Add(new SelectListItem
                        {
                            Value = country.Name,
                            Text = country.Name,
                            Selected = country.Name.Equals(countryChoose) ? true : false
                        });
                    }

                    // Updated 08312017
                    model.ListTimezones = GetTimezonesOfCountry(lstCountries, countryChoose, timeZoneChoose);

                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Edit", model);
                }
                var tmp = model.PictureByte;
                model.PictureByte = null;
                var result = _factory.InsertOrUpdateStore(model);
                if (result)
                {
                    if (!string.IsNullOrEmpty(model.ImageURL) && tmp != null)
                    {
                        FTP.Upload(model.ImageURL, tmp);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    // Get list Countries
                    // Updated 08292017
                    List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
                    foreach (var country in lstCountries)
                    {
                        model.ListCountries.Add(new SelectListItem
                        {
                            Value = country.Name,
                            Text = country.Name,
                            Selected = country.Name.Equals(countryChoose) ? true : false
                        });
                    }

                    // Updated 08312017
                    model.ListTimezones = GetTimezonesOfCountry(lstCountries, countryChoose, timeZoneChoose);

                    return PartialView("_Edit", model);
                }
            }
            catch (FormatException fEx)
            {
                _logger.Error("StoreInformations_Edit: " + fEx.Message);
                ModelState.AddModelError("name", fEx.Message);
                model = GetDetail(model.ID);

                // Get list Countries
                // Updated 08292017
                List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
                foreach (var country in lstCountries)
                {
                    model.ListCountries.Add(new SelectListItem
                    {
                        Value = country.Name,
                        Text = country.Name,
                        Selected = country.Name.Equals(model.Country) ? true : false
                    });
                }

                // Updated 08312017
                model.ListTimezones = GetTimezonesOfCountry(lstCountries, model.Country, model.TimeZone);

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
            catch (Exception ex)
            {
                _logger.Error("StoreInformations_Edit: " + ex);
                ModelState.AddModelError("name", ex.Message);
                model = GetDetail(model.ID);

                // Get list Countries
                // Updated 08292017
                List<CountryApiModels> lstCountries = CommonHelper.GetListCountry();
                foreach (var country in lstCountries)
                {
                    model.ListCountries.Add(new SelectListItem
                    {
                        Value = country.Name,
                        Text = country.Name,
                        Selected = country.Name.Equals(model.Country) ? true : false
                    });
                }

                // Updated 08312017
                model.ListTimezones = GetTimezonesOfCountry(lstCountries, model.Country, model.TimeZone);

                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Edit", model);
            }
        }

        public ActionResult Export()
        {
            SStoreModels model = new SStoreModels();
            return View(model);
        }

        [HttpPost]
        public ActionResult Export(SStoreModels model)
        {
            try
            {
                if (model.ListStores == null)
                {
                    ModelState.AddModelError("ListStores", CurrentUser.GetLanguageTextFromKey("Please choose store"));
                    return View(model);
                }

                XLWorkbook wb = new XLWorkbook();
                var wsSetMenu = wb.Worksheets.Add("Sheet1");
                Shared.Models.StatusResponse response = _factory.Export(ref wsSetMenu, model.ListStores);

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
                Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx",
                    CommonHelper.GetExportFileName("StoreInformation").Replace(" ", "_")));

                using (var memoryStream = new System.IO.MemoryStream())
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
                _logger.Error("StoreInformations_Export: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }
        }

        // Updated 08312017
        public List<SelectListItem> GetTimezonesOfCountry(List<CountryApiModels> ListCountries, string Country, string Timezone)
        {
            List<SelectListItem> listTimezones = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(Country))
            {
                var countryInfo = ListCountries.Where(w => w.Name.Equals(Country)).FirstOrDefault();
                if (countryInfo != null)
                {
                    foreach (string timeZone in countryInfo.Timezones)
                    {
                        listTimezones.Add(new SelectListItem
                        {
                            Text = timeZone,
                            Value = timeZone,
                            Selected = timeZone.Equals(Timezone) ? true : false
                        });
                    }
                }
            }
            return listTimezones;
        }

        // Updated 08312017
        public ActionResult LoadTimezones(string Country)
        {
            List<CountryApiModels> listCountries = CommonHelper.GetListCountry();
            SStoreModels model = new SStoreModels();
            var countryInfo = listCountries.Where(w => w.Name.Equals(Country)).FirstOrDefault();
            if(countryInfo != null)
            {
                foreach(string timeZone in countryInfo.Timezones)
                {
                    model.ListTimezones.Add(new SelectListItem
                    {
                        Text = timeZone,
                        Value = timeZone,
                        Selected = false
                    });
                }
            }
            return PartialView("~/Views/SStoreInformation/_DropDownListTimezones.cshtml", model);
        }
    }
}