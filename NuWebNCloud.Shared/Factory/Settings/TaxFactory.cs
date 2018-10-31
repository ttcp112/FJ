using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class TaxFactory : BaseFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public TaxFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<Models.Settings.TaxModels> GetListTax(string StoreID = null, string ID = null, List<string> ListOrganizationId = null)
        {
            List<Models.Settings.TaxModels> listdata = new List<Models.Settings.TaxModels>();
            try
            {
                TaxApiModels paraBody = new TaxApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;
                paraBody.ListOrgID = ListOrganizationId;
                NSLog.Logger.Info("GetListTax request ", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetTax, null, paraBody);
                NSLog.Logger.Info("GetListTax result ", result);

                dynamic data = result.Data;
                var lstZ = data["ListTax"];
                var lstContent = JsonConvert.SerializeObject(lstZ);
                listdata = JsonConvert.DeserializeObject<List<Models.Settings.TaxModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listdata = listdata.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();
                listdata = listdata.OrderBy(oo => oo.Name).ToList();
                return listdata;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("Tax_GetList Error ", e);
                return listdata;
            }

        }

        public bool InsertOrUpdateTax(Models.Settings.TaxModels model, ref string msg)
        {
            try
            {
                TaxApiModels paraBody = new TaxApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                paraBody.Name = model.Name;
                paraBody.Percent = model.Percent;
                paraBody.TaxType = model.TaxType;
                paraBody.IsActive = model.IsActive;
                paraBody.ListProductID = model.ListProductID;

                NSLog.Logger.Info("InsertOrUpdateTax request", paraBody);
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Tax_InsertOrUpdate_Web, null, paraBody);
                NSLog.Logger.Info("InsertOrUpdateTax result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("Tax_InsertOrUpdate error", e);
                return false;
            }
        }

        public bool DeleteTax(string ID, ref string msg)
        {
            try
            {
                TaxApiModels paraBody = new TaxApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteTax, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
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
                msg = e.ToString();
                _logger.Error("Taxs_Delete: " + e);
                return false;
            }
        }

        //=======V3-version 2
        public List<Models.Settings.TaxModels> GetListTaxV2(string StoreID = null, string ID = null, List<string> ListOrganizationId = null,bool IsWeb = false)
        {
            List<Models.Settings.TaxModels> listdata = new List<Models.Settings.TaxModels>();
            try
            {
                TaxApiModels paraBody = new TaxApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;
                paraBody.ListOrgID = ListOrganizationId;
                paraBody.IsWeb = IsWeb;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetTax_V2, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListTax"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<Models.Settings.TaxModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listdata = listdata.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("Tax_GetList: " + e);
                return listdata;
            }
        }

        public List<ProductOfTaxDTO> CheckProductOnTax(string StoreID = null, string ID = null, List<string> ListProductID = null, int TaxType = 0)
        {
            List<ProductOfTaxDTO> listdata = new List<ProductOfTaxDTO>();
            try
            {
                TaxApiModels paraBody = new TaxApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.StoreID = StoreID;
                paraBody.TaxType = TaxType;
                paraBody.ListProductID = ListProductID;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Check_Product_Tax, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListProductOfTax"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<ProductOfTaxDTO>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("CheckProductOnTax: " + e);
                return listdata;
            }
        }

        public bool InsertOrUpdateTax_V2(Models.Settings.TaxModels model, ref string msg)
        {
            try
            {
                TaxApiModels paraBody = new TaxApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.Type = model.Type;
                //paraBody.Tax = model;
                ////paraBody.ID = model.ID;
                //paraBody.StoreID = model.StoreID;
                ////paraBody.Name = model.Name;
                ////paraBody.Percent = model.Percent;
                ////paraBody.TaxType = model.TaxType;
                ////paraBody.IsActive = model.IsActive;
                ////paraBody.ListProductID = model.ListProductID;

                // Updated 07092018, change Api Tax_V2 for Web
                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                paraBody.Name = model.Name;
                paraBody.Code = model.Code;
                paraBody.Percent = model.Percent;
                paraBody.TaxType = model.TaxType;
                paraBody.IsActive = model.IsActive;
                paraBody.ListProductID = model.ListProductID;
                paraBody.Rate = model.Rate;
                //====================
                NSLog.Logger.Info("InsertOrUpdateTax_V2 request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.Tax_InsertOrUpdate_Web_V2, null, paraBody);
                NSLog.Logger.Info("InsertOrUpdateTax_V2 result", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                      
                        msg = result.Message;
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
                _logger.Error("Tax_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeleteTax_V2(string ID, ref string msg)
        {
            try
            {
                TaxApiModels paraBody = new TaxApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteTax_V2, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
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
                msg = e.ToString();
                _logger.Error("Taxs_Delete: " + e);
                return false;
            }
        }
        //=======End V3-version 2
        // IMPORT
        public StatusResponse Import(HttpPostedFileBase excelFile, List<string> lstStore, ref ImportModel importModel, ref string msg)
        {
            StatusResponse Response = new StatusResponse();
            DataTable dt = new DataTable();
            FileInfo[] lstFileImg = new FileInfo[] { };
            Response = ProcessDataImport(ref dt, excelFile, "SBSettingTax.xlsx", out lstFileImg);

            if (!Response.Status)
            {
                msg = Response.MsgError;
                return Response;
            }

            ImportItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            List<Models.Settings.TaxModels> listData = new List<Models.Settings.TaxModels>();
            foreach (var item in lstStore)
            {
                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        flagInsert = true;
                        msgError = "";

                        string rowText = "";

                        for (int i = 0; i < dt.Columns.Count; i++)
                            rowText += row[i].ToString().Trim();

                        if (string.IsNullOrEmpty(rowText))
                            continue;

                        double doubleVal = 0;
                        Models.Settings.TaxModels model = new Models.Settings.TaxModels();
                        model.Index = row[0].ToString();
                        // 1 - Name
                        model.Name = row[1].ToString().Trim().Replace("  ", " ");
                        // 2 Code
                        model.Code = row[2].ToString().Trim().Replace("  ", " ");
                        // 3 - IsActive
                        model.IsActive = GetBoolValue(dt.Columns[3].ColumnName.Replace("#", "."), row[0].ToString(), row[3].ToString());
                        // 4 - Percent (%)
                        double.TryParse(row[4].ToString(), out doubleVal);
                        model.Percent = (int)doubleVal;
                        // 5 - Zone
                        model.TaxType = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(row[5].ToString().ToLower()).Contains(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("include")) ? (byte)Commons.ETax.Inclusive :
                                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(row[5].ToString().ToLower()).Contains(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("add")) ? (byte)Commons.ETax.AddOn : 0;
                        // 6 - Store
                        model.StoreID = item;
                        model.StoreName = row[6].ToString().Trim().Replace("  ", " ");
                        //==============
                        if (row[4].ToString().Replace(" ", "").Equals(""))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent is required");
                        }
                        if (model.Percent < 0 || model.Percent > 100)
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent must between 0 and 100");
                        }
                        if (listData.Count > 0)
                        {
                            var IsExists = listData.Exists(x => x.IsActive && x.StoreID.Equals(model.StoreID));
                            if (IsExists)
                            {
                                if (model.IsActive)
                                {
                                    flagInsert = false;
                                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Duplicate active tax in store");
                                }
                            }
                        }
                        if (flagInsert)
                        {
                            listData.Add(model);
                        }
                        else
                        {
                            itemErr = new ImportItem();
                            itemErr.Name = model.Name;
                            itemErr.ListFailStoreName.Add("");
                            itemErr.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + model.Index + msgError);
                            importModel.ListImport.Add(itemErr);
                        }
                    }
                    catch (Exception e)
                    {
                        importModel.ListImport.Add(new ImportItem { Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax"), ListFailStoreName = lstStore, ListErrorMsg = new List<string> { e.Message } });
                    }
                }
            }
            Response.Status = true;
            //=====================

            //try
            //{
            TaxApiModels paraBody = new TaxApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListTax = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportTax, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                foreach (ImportResult itemError in listError)
                {
                    ImportItem item = new ImportItem();
                    item.Name = itemError.Property;
                    item.ListFailStoreName.Add(itemError.StoreName);
                    item.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index + "<br/>"
                        + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error));
                    importModel.ListImport.Add(item);
                }
                if (importModel.ListImport.Count == 0)
                {
                    ImportItem item = new ImportItem();
                    item.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax List");
                    item.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Tax Successful"));
                    importModel.ListImport.Add(item);
                }
            }
            return Response;
        }

        public StatusResponse Export(ref IXLWorksheet ws, List<string> lstStore)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<Models.Settings.TaxModels> listData = new List<Models.Settings.TaxModels>();

                TaxApiModels paraBody = new TaxApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportTax, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListTax"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<Models.Settings.TaxModels>>(lstContent);
                int cols = 7;
                //Header
                int row = 1;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index");
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Name");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Tax Code");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent (%)");
                ws.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type");
                ws.Cell("G" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name");
                //Item
                row = 2;
                int countIndex = 1;
                listData = listData.OrderBy(oo => oo.StoreName).ThenBy(o => o.Name).ToList();
                foreach (var item in listData)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = item.Code;
                    ws.Cell("D" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");
                    ws.Cell("E" + row).Value = item.Percent;
                    ws.Cell("F" + row).Value = (item.TaxType == (byte)Commons.ETax.AddOn) ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETaxAddOnTax.ToString()) :
                                                (item.TaxType == (byte)Commons.ETax.Inclusive) ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETaxIncludeTax.ToString()) : "";
                    ws.Cell("G" + row).Value = item.StoreName;
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
    }
}
