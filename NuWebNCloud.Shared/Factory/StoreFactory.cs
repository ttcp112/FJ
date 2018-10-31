using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Data;

namespace NuWebNCloud.Shared.Factory
{
    public class StoreFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        //private UserSession usersesion; 
        private BaseFactory _baseFactory = null;
        public StoreFactory()
        {
            _baseFactory = new BaseFactory();
            //usersesion = new UserSession();
        }

        public List<SelectListItem> GetListStore(List<SelectListItem> lstCompany)
        {
            List<SelectListItem> listStores = new List<SelectListItem>();
            try
            {
                //check to session
                //if (System.Web.HttpContext.Current.Session["GetStoreListSession"] != null)
                //{
                //    listStores = (List<SelectListItem>)System.Web.HttpContext.Current.Session["GetStoreListSession"];
                //}
                //else
                //{
                if (lstCompany.Count > 0)
                {
                    for (int j = 0; j < lstCompany.Count; j++)
                    {
                        string CompanyID = lstCompany[j].Value;
                        StoreApiModels paraBody = new StoreApiModels();
                        paraBody.CompanyID = CompanyID;
                        var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetStore, null, paraBody);
                        dynamic data = result.Data;
                        var lstC = data["ListStore"];
                        var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                        var listItems = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                        if (listItems.Count != 0)
                        {
                            for (int i = 0; i < listItems.Count; i++)
                            {
                                var item = listItems[i];
                                listStores.Add(new SelectListItem()
                                {
                                    Value = item.Id.ToString(),
                                    Text = item.Name,
                                });

                                // Add value common for FJ Daily Sales report
                                if (item.StoreCode == Commons.Stall14StoreCode)
                                {
                                    Commons.Stall14StoreId = item.Id.ToString();
                                    Commons.Stall14StoreName = item.Name;
                                }
                            }
                        }
                    }
                    var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                    if (currentUser != null)
                        listStores = listStores.Where(x => currentUser.ListStoreID.Contains(x.Value)).ToList();
                    if (listStores != null && listStores.Any())
                        listStores = listStores.OrderBy(s => s.Text).ToList();
                    //if (listStores != null && listStores.Any())
                    //{
                    //    HttpContext.Current.Session.Add("GetStoreListSession", listStores);
                    //}
                }
                //}
                return listStores;
            }
            catch (Exception e)
            {
                _logger.Error("Get store list: " + e);
                return listStores;
            }
        }

        public List<SelectListItem> GetListStoreForMerchantExtend(string hostUrl, List<string> lstOrgId, List<MerchantExtendModel> lstMerchantExtends, ref List<StoreModels> listStoreReturn)
        {

            List<SelectListItem> listStores = new List<SelectListItem>();
            List<StoreModels> listData = new List<StoreModels>();
            try
            {
                
                GetStoreWeb2Request paraBody = new GetStoreWeb2Request();
                paraBody.ListOrgID = lstOrgId;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Store_Get_Web2, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListStore"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                //listData.ForEach(ss => ss.HostUrlExtend = hostUrl);
                foreach (var item in listData)
                {
                    item.HostUrlExtend = hostUrl;
                    item.NameExtend = string.Format("{0} in {1}", item.Name, item.OrganizationName);
                }
                //for extend
                foreach (var item in lstMerchantExtends)
                {
                    paraBody = new GetStoreWeb2Request();
                    paraBody.ListStoreID = item.ListStoreIds;
                    result = (ResponseApiModels)ApiResponse.PostWithoutHostConfig<ResponseApiModels>(item.HostApiURL + "/" + Commons.Store_Get_Web2, null, paraBody);
                    data = result.Data;
                    lstC = data["ListStore"];
                    lstContent = JsonConvert.SerializeObject(lstC);
                    List<StoreModels> tmp = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                    foreach (var subitem in tmp)
                    {
                        subitem.HostUrlExtend = item.HostApiURL;
                        subitem.NameExtend = string.Format("{0} in {1}", subitem.Name, subitem.OrganizationName);
                    }
                    //tmp.ForEach(ss => ss.HostUrlExtend = item.HostApiURL, SS);
                    listData.AddRange(tmp);
                }


                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listData = listData.Where(x => currentUser.ListStoreID.Contains(x.Id)).ToList();

                listStoreReturn = listData;

                listStores = listData.Select(ss => new SelectListItem()
                {
                    Value = ss.Id,
                    Text = ss.Name
                }).ToList();

                //for  FJ Daily Sales report
                StoreModels stall14 = listData.Where(ww => ww.StoreCode == Commons.Stall14StoreCode).FirstOrDefault();
                if (stall14 != null)
                {
                    Commons.Stall14StoreId = stall14.Id;
                    Commons.Stall14StoreName = stall14.Name;
                }

             
                return listStores;
            }
            catch (Exception e)
            {
                _logger.Error("Get store extend list: " + e);
                return listStores;
            }
        }

        public List<StoreModels> GetListStoreRole(List<string> ListOrganizationId = null, List<string> ListStoreId = null)
        {
            List<StoreModels> listStores = new List<StoreModels>();
            try
            {
                if ((ListOrganizationId != null && ListOrganizationId.Any()) || (ListStoreId != null && ListStoreId.Any()))
                {
                    GetStoreWeb2Request paraBody = new GetStoreWeb2Request();
                    paraBody.ListOrgID = ListOrganizationId;
                    paraBody.ListStoreID = ListStoreId;
                    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Store_Get_Web2, null, paraBody);
                    dynamic data = result.Data;
                    var lstC = data["ListStore"];
                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                    listStores = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                    var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                    if (currentUser != null)
                        listStores = listStores.Where(x => currentUser.ListStoreID.Contains(x.Id)).ToList();
                    if (listStores != null && listStores.Any())
                        listStores = listStores.OrderBy(s => s.Name).ToList();
                }
                return listStores;
            }
            catch (Exception e)
            {
                _logger.Error("Get store list role: " + e);
                return listStores;
            }
        }

        public List<SelectListItem> GetListStore(List<string> ListOrganizationId = null, List<string> ListStoreId = null)
        {
            List<SelectListItem> listStores = new List<SelectListItem>();
            try
            {
                if ((ListOrganizationId != null && ListOrganizationId.Any()) || (ListStoreId != null && ListStoreId.Any()))
                {
                    GetStoreWeb2Request paraBody = new GetStoreWeb2Request();
                    paraBody.ListOrgID = ListOrganizationId;
                    paraBody.ListStoreID = ListStoreId;
                    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Store_Get_Web2, null, paraBody);
                    dynamic data = result.Data;
                    var lstC = data["ListStore"];
                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                    var listItems = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                    if (listItems.Count != 0)
                    {
                        for (int i = 0; i < listItems.Count; i++)
                        {
                            var item = listItems[i];
                            listStores.Add(new SelectListItem()
                            {
                                Value = item.Id.ToString(),
                                Text = item.Name
                            });
                            // Add value common for FJ Daily Sales report
                            if (item.StoreCode == Commons.Stall14StoreCode)
                            {
                                Commons.Stall14StoreId = item.Id.ToString();
                                Commons.Stall14StoreName = item.Name;
                            }
                        }
                    }
                    var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                    if (currentUser != null)
                        listStores = listStores.Where(x => currentUser.ListStoreID.Contains(x.Value)).ToList();
                    if (listStores != null && listStores.Any())
                        listStores = listStores.OrderBy(s => s.Text).ToList();
                }
                return listStores;
            }
            catch (Exception e)
            {
                _logger.Error("Get store list: " + e);
                return listStores;
            }
        }

        public List<StoreModels> GetListStores(List<string> ListOrganizationId = null, List<string> ListStoreId = null)
        {
            List<StoreModels> listStores = new List<StoreModels>();
            try
            {
                if ((ListOrganizationId != null && ListOrganizationId.Any()) || (ListStoreId != null && ListStoreId.Any()))
                {
                    GetStoreWeb2Request paraBody = new GetStoreWeb2Request();
                    paraBody.ListOrgID = ListOrganizationId;
                    paraBody.ListStoreID = ListStoreId;
                    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Store_Get_Web2, null, paraBody);
                    dynamic data = result.Data;
                    var lstC = data["ListStore"];
                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);                    
                    listStores = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                   
                    var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                    if (currentUser != null)
                        listStores = listStores.Where(x => currentUser.ListStoreID.Contains(x.Id)).ToList();
                    if (listStores != null && listStores.Any())
                        listStores = listStores.OrderBy(s => s.Name).ToList();
                }
                return listStores;
            }
            catch (Exception e)
            {
                _logger.Error("Get store list: " + e);
                return listStores;
            }
        }

        public List<StoreModels> GetListStoreForTranfer(List<string> ListOrganizationId = null, List<string> ListStoreId = null)
        {
            List<StoreModels> listStores = new List<StoreModels>();
            try
            {
                if ((ListOrganizationId != null && ListOrganizationId.Any()) || (ListStoreId != null && ListStoreId.Any()))
                {
                    GetStoreWeb2Request paraBody = new GetStoreWeb2Request();
                    paraBody.ListOrgID = ListOrganizationId;
                    paraBody.ListStoreID = ListStoreId;
                    var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Store_Get_Web2, null, paraBody);
                    dynamic data = result.Data;
                    var lstC = data["ListStore"];
                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                    var listItems = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                    //if (listItems.Count != 0)
                    //{
                    //    for (int i = 0; i < listItems.Count; i++)
                    //    {
                    //        var item = listItems[i];
                    //        listStores.Add(new SelectListItem()
                    //        {
                    //            Value = item.Id.ToString(),
                    //            Text = item.Name
                    //        });
                    //    }
                    //}
                    //var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                    //if (currentUser != null)
                    //    listStores = listStores.Where(x => currentUser.ListStoreID.Contains(x.Value)).ToList();
                    if (listItems != null)
                    {
                        listStores.AddRange(listItems);
                    }
                    if (listStores != null && listStores.Any())
                        listStores = listStores.OrderBy(s => s.Name).ToList();
                }
                return listStores;
            }
            catch (Exception e)
            {
                _logger.Error("GetListStoreForTranfer: " + e);
                return listStores;
            }
        }
        public List<SStoreModels> GetListStores(string StoreID = null, string ID = null, List<string> lstOrgs = null)
        {
            List<SStoreModels> listdata = new List<SStoreModels>();
            try
            {
                StoreApiModel paraBody = new StoreApiModel();

                paraBody.ListOrganizations = lstOrgs;
                paraBody.StoreId = StoreID;
                paraBody.Id = (!string.IsNullOrEmpty(ID) ? ID : StoreID);

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetStores, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListStore"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<SStoreModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listdata = listdata.Where(x => currentUser.ListStoreID.Contains(x.ID)).ToList();
                if (listdata != null && listdata.Any())
                    listdata = listdata.OrderBy(oo => oo.Name).ToList();

                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("Store_GetList: " + e);
                return listdata;
            }

        }

        public bool InsertOrUpdateStore(SStoreModels model)
        {
            try
            {
                StoreApiModel paraBody = new StoreApiModel();
                SStoreModels store = new SStoreModels();

                store.ID = model.ID;
                store.Name = model.Name;
                store.ImageURL = model.ImageURL;
                store.Email = model.Email;
                store.Street = model.Street;
                store.City = model.City;
                store.Country = model.Country;
                store.TimeZone = model.TimeZone;
                store.IsActive = model.IsActive;
                store.ListBusinessHour = model.ListBusinessHour;
                store.OrganizationID = model.OrganizationID;
                store.CompanyID = model.CompanyID;
                store.IndustryID = model.IndustryID;
                store.Description = model.Description;
                store.ExceptionData = model.ExceptionData;
                store.Code = model.Code;
                store.Phone = model.Phone;
                store.Zipcode = model.Zipcode;
                store.GSTRegNo = model.GSTRegNo;
                store.StoreCode = model.StoreCode;
                //===================
                paraBody.Store = store;
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.Mode = 1;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.StoreId = model.ID;
                paraBody.Id = model.ID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditStore, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        _logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Stores_InsertOrUpdate: " + e);
                return false;
            }
        }

        public Models.StatusResponse Export(ref IXLWorksheet ws, List<string> lstStore)
        {
            Models.StatusResponse Response = new Models.StatusResponse();
            try
            {
                List<SStoreModels> listData = new List<SStoreModels>();

                StoreApiModel paraBody = new StoreApiModel();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportStore, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListStore"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<SStoreModels>>(lstContent);

                ws.Cell("A" + 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Information").ToString().ToUpper();
                ws.Row(1).Style.Font.SetBold(true);
                ws.Row(1).Height = 20;
                ws.Row(1).Style.Font.FontSize = 15;
                ws.Range(1, 1, 1, 18).Merge();
                ws.Cell("A" + 1).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
                int row = 2;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Phone"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Email"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Street"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Stage/Province"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Country"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Zip Code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GST Reg No"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time Zone"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Monday"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tuesday"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Wednesday"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Thursday"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Friday"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Saturday"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Sunday"),
                };
                for (int i = 1; i <= listSetMenuHeader.Length; i++)
                {
                    ws.Cell(row, i).Value = listSetMenuHeader[i - 1];
                    ws.Cell(row, i).Style.Font.Bold = true;
                }
                int cols = listSetMenuHeader.Length;
                //Item
                row = 3;
                int countIndex = 1;

                foreach (var item in listData)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = "'" + item.Phone;
                    ws.Cell("D" + row).Value = item.Email;
                    ws.Cell("E" + row).Value = item.Street;
                    ws.Cell("F" + row).Value = "'" + item.City;
                    ws.Cell("G" + row).Value = "'" + item.Country;
                    ws.Cell("H" + row).Value = "'" + item.Zipcode;

                    ws.Cell("I" + row).Value = "'" + item.GSTRegNo;
                    ws.Cell("J" + row).Value = "'" + item.TimeZone;
                    ws.Cell("K" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");

                    var b = "OFF-OFF";
                    if (item.ListBusinessHour != null)
                    {
                        item.ListBusinessHour = item.ListBusinessHour.OrderBy(x => x.Day).ToList();
                        item.GetTime();
                        var a = "";
                        for (int i = 0; i < item.ListBusinessHour.Count; i++)
                        {
                            if (!(item.ListBusinessHour[i].IsOffline))
                            {
                                a = item.ListBusinessHour[i].FromTime.ToLocalTime().ToString("HH:mm") + " - " + item.ListBusinessHour[i].ToTime.ToLocalTime().ToString("HH:mm");
                            }
                            else
                            {
                                a = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF") + "-" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("OFF");
                            }
                            //========
                            if (item.ListBusinessHour[i].Day == 2)
                            {
                                ws.Cell("L" + row).Value = a;
                            }
                            else if (item.ListBusinessHour[i].Day == 3)
                            {
                                ws.Cell("M" + row).Value = a;
                            }
                            else if (item.ListBusinessHour[i].Day == 4)
                            {
                                ws.Cell("N" + row).Value = a;
                            }
                            else if (item.ListBusinessHour[i].Day == 5)
                            {
                                ws.Cell("O" + row).Value = a;
                            }
                            else if (item.ListBusinessHour[i].Day == 6)
                            {
                                ws.Cell("P" + row).Value = a;
                            }
                            else if (item.ListBusinessHour[i].Day == 7)
                            {
                                ws.Cell("Q" + row).Value = a;
                            }
                            else if (item.ListBusinessHour[i].Day == 8)
                            {
                                ws.Cell("R" + row).Value = a;
                            }
                        }
                    }
                    else
                    {
                        ws.Cell("L" + row).Value = b;
                        ws.Cell("M" + row).Value = b;
                        ws.Cell("N" + row).Value = b;
                        ws.Cell("O" + row).Value = b;
                        ws.Cell("P" + row).Value = b;
                        ws.Cell("Q" + row).Value = b;
                        ws.Cell("R" + row).Value = b;
                    }
                    row++;
                    countIndex++;
                }
                FormatExcelExport(ws, row, cols);
                Response.Status = true;
            }
            catch (Exception e)
            {
                Response.Status = false;
                Response.MsgError = e.Message;
            }
            finally
            {

            }
            return Response;
        }

        // Get list stores group by company for view
        public List<StoresByCompany> GetListStore_View(List<SelectListItem> lstCompany)
        {
            List<StoresByCompany> listStores = new List<StoresByCompany>();
            List<StoreModels> listItems = new List<StoreModels>();
            try
            {
                if (lstCompany.Count > 0)
                {
                    if (System.Web.HttpContext.Current.Session["GetListStore_View"] != null)
                    {
                        listStores = (List<StoresByCompany>)HttpContext.Current.Session["GetListStore_View"];
                    }
                    else
                    {
                        StoresByCompany groupCompany = new StoresByCompany();
                        var currentUser = (UserSession)HttpContext.Current.Session["User"];
                        for (int j = 0; j < lstCompany.Count; j++)
                        {
                            string CompanyID = lstCompany[j].Value;
                            StoreApiModels paraBody = new StoreApiModels();
                            paraBody.CompanyID = CompanyID;
                            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetStore, null, paraBody);
                            dynamic data = result.Data;
                            var lstC = data["ListStore"];
                            var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                            listItems = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                            if (listItems.Count != 0)
                            {
                                groupCompany = new StoresByCompany();
                                groupCompany.text = lstCompany[j].Text;

                                if (currentUser != null)
                                {
                                    listItems = listItems.Where(w => currentUser.ListStoreID.Contains(w.Id)).ToList();
                                }
                                for (int i = 0; i < listItems.Count; i++)
                                {
                                    var item = listItems[i];
                                    groupCompany.children.Add(new StoresChildren()
                                    {
                                        id = item.Id.ToString(),
                                        text = item.Name
                                    });
                                    // Add value common for FJ Daily Sales report
                                    if (item.StoreCode == Commons.Stall14StoreCode)
                                    {
                                        Commons.Stall14StoreId = item.Id.ToString();
                                        Commons.Stall14StoreName = item.Name;
                                    }
                                }
                                groupCompany.children = groupCompany.children.OrderBy(o => o.text).ToList();
                                listStores.Add(groupCompany);
                            }
                        }
                        if (listStores != null && listStores.Any())
                        {
                            listStores = listStores.OrderBy(o => o.text).ToList();

                            System.Web.HttpContext.Current.Session.Add("GetListStore_View", listStores);
                        }
                    }
                }
                return listStores;
            }
            catch (Exception e)
            {
                _logger.Error("GetListStore_View: " + e);
                return listStores;
            }
        }

        #region 2018-01-31 store include company
        public List<StoreModels> GetListStoreIncludeCompany(List<SelectListItem> lstCompany)
        {
            List<StoreModels> listStores = new List<StoreModels>();
            try
            {

                if (lstCompany.Count > 0)
                {
                    for (int j = 0; j < lstCompany.Count; j++)
                    {
                        string CompanyID = lstCompany[j].Value;
                        StoreApiModels paraBody = new StoreApiModels();
                        paraBody.CompanyID = CompanyID;
                        var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetStore, null, paraBody);
                        dynamic data = result.Data;
                        var lstC = data["ListStore"];
                        var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                        List<StoreModels> lstTmp = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                        if (lstTmp != null && lstTmp.Count != 0)
                        {
                            var stall14 = lstTmp.Where(ww => ww.StoreCode == Commons.Stall14StoreCode).FirstOrDefault();
                            // Add value common for FJ Daily Sales report
                            if (stall14 != null)
                            {
                                Commons.Stall14StoreId = stall14.Id;
                                Commons.Stall14StoreName = stall14.Name;
                            }
                            listStores.AddRange(lstTmp);
                        }
                    }
                }
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listStores = listStores.Where(x => currentUser.ListStoreID.Contains(x.Id)).ToList();
                if (listStores != null && listStores.Any())
                {
                    TaxFactory taxFactory = new TaxFactory();
                    listStores = listStores.OrderBy(s => s.CompanyName).ThenBy(aa=>aa.Name).ToList();
                    foreach (var store in listStores)
                    {
                        store.IsIncludeTax = (taxFactory.GetDetailTaxForStore(store.Id) == (int)Commons.ETax.AddOn) ? false : true;
                    }
                }

                return listStores;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetListStoreIncludeCompany error: ", e);
                return listStores;
            }
        }
        #endregion

        #region Update 04132018, Get list stores group by company for all view select list stores (from session ["GetListStore_View_V1"] or get new data)
        public List<StoreModels> GetListStore_View_V1(List<SelectListItem> lstCompany = null)
        {
            List<StoreModels> listStores = new List<StoreModels>();
            List<StoreModels> listItems = new List<StoreModels>();
            try
            {
                if (System.Web.HttpContext.Current.Session["GetListStore_View_V1"] != null)
                {
                    listStores = (List<StoreModels>)HttpContext.Current.Session["GetListStore_View_V1"];
                }
                else
                {
                    var currentUser = (UserSession)HttpContext.Current.Session["User"];
                    if (currentUser != null)
                    {
                        if (lstCompany == null || !lstCompany.Any())
                        {
                            var listOrganizationId = currentUser.ListOrganizationId.ToList();
                            CompanyFactory companyFactory = new CompanyFactory();
                            lstCompany = companyFactory.GetListCompany(listOrganizationId);
                        }
                        if (lstCompany != null && lstCompany.Any())
                        {
                            for (int j = 0; j < lstCompany.Count; j++)
                            {
                                string CompanyID = lstCompany[j].Value;
                                StoreApiModels paraBody = new StoreApiModels();
                                paraBody.CompanyID = CompanyID;
                                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetStore, null, paraBody);
                                dynamic data = result.Data;
                                var lstC = data["ListStore"];
                                var lstContent = JsonConvert.SerializeObject(lstC);
                                listItems = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);
                                if (listItems != null && listItems.Any())
                                {
                                    if (currentUser != null)
                                    {
                                        listItems = listItems.Where(w => currentUser.ListStoreID.Contains(w.Id)).ToList();
                                    }
                                    // Add value common for FJ Daily Sales report
                                    var stall14 = listItems.Where(w => w.StoreCode == Commons.Stall14StoreCode).FirstOrDefault();
                                    if (stall14 != null)
                                    {
                                        Commons.Stall14StoreId = stall14.Id.ToString();
                                        Commons.Stall14StoreName = stall14.Name;
                                    }
                                    listStores.AddRange(listItems);
                                }
                            }
                            if (listStores != null && listStores.Any())
                            {
                                listStores = listStores.OrderBy(o => o.Name).ToList();

                                System.Web.HttpContext.Current.Session.Add("GetListStore_View_V1", listStores);
                            }
                        }
                        
                    }
                }

                return listStores;
            }
            catch (Exception e)
            {
                _logger.Error("GetListStore_View_V1: " + e);
                return listStores;
            }
        }
        #endregion

        #region Updated 07172018, list stores info from Session
        public List<StoreModels> GetListStoresInfo(List<string> lstOrgId = null)
        {
            //using (var db = new NuWebContext())
            //{
            //    var listItemInRecipe = (from ri in db.I_Recipe_Item
            //                            join i in db.I_Ingredient on ri.IngredientId equals i.Id
            //                            where ri.Status != (int)Commons.EStatus.Deleted
            //                            && i.IsCheckStock
            //                            group ri by ri.ItemId into g
            //                            where g.Count() == 1
            //                            select new { g.Key }
            //                            ).ToList();

            //}

            List<StoreModels> listStores = new List<StoreModels>();
            List<StoreModels> listItems = new List<StoreModels>();
            try
            {
                if (System.Web.HttpContext.Current.Session["ListStoresInfo"] != null)
                {
                    listStores = (List<StoreModels>)System.Web.HttpContext.Current.Session["ListStoresInfo"];
                }
                else
                {
                    var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                    if (currentUser != null)
                    {
                        var listOrganizationId = lstOrgId;
                        if (listOrganizationId == null)
                        {
                            listOrganizationId = currentUser.ListOrganizationId.ToList();
                        }

                        if (listOrganizationId != null && listOrganizationId.Any())
                        {
                            // Tax info 
                            NuWebNCloud.Shared.Factory.Settings.TaxFactory taxFactory = new Settings.TaxFactory();
                            List<Models.Settings.TaxModels> lstTax = new List<Models.Settings.TaxModels>();

                            //=== Get list companies info ===//
                            ////=== Merchant not extend
                            // Get list stores info of listOrganizationId
                            GetStoreWeb2Request paraBody = new GetStoreWeb2Request();
                            paraBody.ListOrgID = listOrganizationId;
                            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Store_Get_Web2, null, paraBody);
                            dynamic data = result.Data;
                            var lstC = data["ListStore"];
                            var lstContent = JsonConvert.SerializeObject(lstC);
                            List<StoreModels> lstStores = JsonConvert.DeserializeObject<List<StoreModels>>(lstContent);

                            string hostUrlExtend = null;
                            if (!currentUser.IsMerchantExtend)
                            {
                                var stall14 = lstStores.Where(ww => ww.StoreCode == Commons.Stall14StoreCode).FirstOrDefault();
                                // Add value common for FJ Daily Sales report
                                if (stall14 != null)
                                {
                                    Commons.Stall14StoreId = stall14.Id;
                                    Commons.Stall14StoreName = stall14.Name;
                                }

                                // Get list tax info
                                lstTax = taxFactory.GetListTax(null, null, listOrganizationId);
                            }
                            else
                            {
                                hostUrlExtend = currentUser.HostApi;
                                ////=== Merchant extend
                                // Get list stores info of ListStoreIds
                                if (currentUser.ListMerchantExtends != null && currentUser.ListMerchantExtends.Any())
                                {
                                    foreach (var item in currentUser.ListMerchantExtends)
                                    {

                                        var paraBodyExtend = new GetStoreWeb2Request();
                                        paraBodyExtend.ListStoreID = item.ListStoreIds;
                                        var resultExtend = (ResponseApiModels)ApiResponse.PostWithoutHostConfig<ResponseApiModels>(item.HostApiURL + "/" + Commons.Store_Get_Web2, null, paraBodyExtend);
                                        dynamic dataExtend = resultExtend.Data;
                                        var lstExtend = dataExtend["ListStore"];
                                        var lstContentExtend = JsonConvert.SerializeObject(lstExtend);
                                        List<StoreModels> lstStoresExtend = JsonConvert.DeserializeObject<List<StoreModels>>(lstContentExtend);

                                        if (lstStoresExtend != null && lstStoresExtend.Any())
                                        {
                                            foreach (var store in lstStoresExtend)
                                            {
                                                listStores.Add(new StoreModels()
                                                {
                                                    Id = store.Id,
                                                    Name = store.Name,
                                                    CompanyId = store.CompanyId,
                                                    CompanyName = store.CompanyName,
                                                    HostUrlExtend = item.HostApiURL,
                                                    IsIncludeTax = store.IsIncludeTax,
                                                    OrganizationId = store.OrganizationId,
                                                    OrganizationName = store.OrganizationName,
                                                    NameExtend = string.Format("{0} in {1}", store.Name, store.OrganizationName),
                                                });
                                            }
                                        }
                                    }
                                }
                            }

                            lstStores = lstStores.Where(x => currentUser.ListStoreID.Contains(x.Id)).ToList();

                            string nameExtend = "";
                            bool isIncludeTax = true;
                            foreach (var store in lstStores)
                            {
                                if (!currentUser.IsMerchantExtend)
                                {
                                    int taxType = lstTax.Where(an => an.StoreID == store.Id && an.IsActive).OrderBy(oo => oo.Name).Select(ss => ss.TaxType).FirstOrDefault();
                                    isIncludeTax = (taxType == (int)Commons.ETax.AddOn) ? false : true;
                                }
                                else
                                {
                                    isIncludeTax = store.IsIncludeTax;
                                    nameExtend = string.Format("{0} in {1}", store.Name, store.OrganizationName);
                                }
                                listStores.Add(new StoreModels()
                                {
                                    Id = store.Id,
                                    Name = store.Name,
                                    CompanyId = store.CompanyId,
                                    CompanyName = store.CompanyName,
                                    HostUrlExtend = hostUrlExtend,
                                    IsIncludeTax = isIncludeTax,
                                    OrganizationId = store.OrganizationId,
                                    OrganizationName = store.OrganizationName,
                                    NameExtend = nameExtend,
                                });
                            }

                            if (listStores != null && listStores.Any())
                            {
                                listStores = listStores.OrderBy(o => o.CompanyName).ThenBy(oo => oo.Name).ToList();

                                System.Web.HttpContext.Current.Session.Add("ListStoresInfo", listStores);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("GetListStoresInfo: " + e);
            }
            return listStores;
        }
        #endregion Updated 07172018, list stores info from Session
    }
}