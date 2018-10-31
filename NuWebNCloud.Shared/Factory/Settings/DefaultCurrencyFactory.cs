using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings.DefaultCurency;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class DefaultCurrencyFactory : BaseFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public DefaultCurrencyFactory()
        {
            _baseFactory = new BaseFactory();
        }
        //GetListDefaultCurrency
        public List<DefaultCurrencyModels> GetListDefaultCurrency(string StoreID = null, string ID = null, List<string> ListOrganizationId = null)
        {
            List<DefaultCurrencyModels> listdata = new List<DefaultCurrencyModels>();
            try
            {
                DefaultCurrencyApiModels paraBody = new DefaultCurrencyApiModels();

                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                //paraBody.Mode = 1;

                paraBody.StoreId = StoreID;
                paraBody.Id = ID;
                paraBody.ListOrgID = ListOrganizationId;
                NSLog.Logger.Info("GetListDefaultCurrency request", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCurrency, null, paraBody);
                NSLog.Logger.Info("GetListDefaultCurrency response", result);
                dynamic data = result.Data;
                var lstZ = data["ListCurrency"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<DefaultCurrencyModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listdata = listdata.Where(ww => currentUser.ListStoreID.Contains(ww.StoreId)).ToList();
                return listdata;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("DefaultCurrency_GetList Error: ", e);
                return listdata;
            }

        }
        //InsertOrUpdateDefaultCurrency
        public bool InsertOrUpdateDefaultCurrency(DefaultCurrencyModels model, ref string msg)
        {
            try
            {
                DefaultCurrencyApiModels paraBody = new DefaultCurrencyApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                //-----

                paraBody.StoreId = model.StoreId;
                paraBody.Id = model.Id;

                paraBody.IsSelected = model.IsSelected;
                paraBody.Name = model.Name;
                paraBody.Symbol = model.Symbol;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditCurrency, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        _logger.Error(result.Message);
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
                _logger.Error("DefaultCurrency_InsertOrUpdate: " + e);
                return false;
            }
        }

        //Deleted

        public bool DeleteDefaultCurrency(string ID, ref string msg)
        {
            try
            {
                DefaultCurrencyApiModels paraBody = new DefaultCurrencyApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.Id = ID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteCurrency, null, paraBody);
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
                _logger.Error("Default Currency_Delete: " + e);
                return false;
            }
        }
        // IMPORT
        public StatusResponse Import(HttpPostedFileBase excelFile, List<string> lstStore, ref ImportModel importModel, ref string msg)
        {
            StatusResponse Response = new StatusResponse();
            DataTable dt = new DataTable();
            FileInfo[] lstFileImg = new FileInfo[] { };
            Response = ProcessDataImport(ref dt, excelFile, "SBSettingCurrency.xlsx", out lstFileImg);

            if (!Response.Status)
            {
                msg = Response.MsgError;
                return Response;
            }

            List<DefaultCurrencyModels> listData = new List<DefaultCurrencyModels>();
            ImportItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

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
                        DefaultCurrencyModels model = new DefaultCurrencyModels();

                        model.Index = row[0].ToString();
                        // 1 - Currency Name
                        model.Name = row[1].ToString().Trim().Replace("  ", " ");
                        // 2 - IsActive
                        model.IsSelected = string.IsNullOrEmpty(row[3].ToString()) ? false :
                                                _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(row[3].ToString()).Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("selected")) ? true : false;
                        // 3 - Symbol
                        model.Symbol = row[2].ToString().Trim().Equals("") ? "" : row[2].ToString().Trim();
                        model.StoreId = item;

                        if (model.Name.Equals(""))
                        {
                            flagInsert = false;
                            msgError += _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name is required");
                        }
                        if (model.Symbol.Equals(""))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Symbol is required");
                        }
                        if (listData.Any(x => x.Name.Equals(model.Name)))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name is exist");
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
                        importModel.ListImport.Add(new ImportItem { Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency"), ListFailStoreName = lstStore, ListErrorMsg = new List<string> { e.Message } });
                    }
                }
            }
            Response.Status = true;
            //=====================
            //try
            //{
            DefaultCurrencyApiModels paraBody = new DefaultCurrencyApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListCurrency = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportCurrency, null, paraBody);
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
                    item.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index + "<br/>" + itemError.Error);
                    importModel.ListImport.Add(item);
                }
                if (importModel.ListImport.Count == 0)
                {
                    ImportItem item = new ImportItem();
                    item.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency");
                    item.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Currency Successful"));
                    importModel.ListImport.Add(item);
                }
            }
            return Response;
        }

        public StatusResponse Import(object excelUpload, List<string> listStores, ref ImportModel importModel)
        {
            throw new NotImplementedException();
        }

        //Export DefaultCurrency
        public StatusResponse Export(ref IXLWorksheet ws, List<string> lstStore)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<DefaultCurrencyModels> listData = new List<DefaultCurrencyModels>();

                DefaultCurrencyApiModels paraBody = new DefaultCurrencyApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportCurrency, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListCurrency"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<DefaultCurrencyModels>>(lstContent);

                listData = listData.OrderBy(o => o.StoreName).ToList();
                int row = 1;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency Symbol"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selected"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store")
                };
                for (int i = 1; i <= listSetMenuHeader.Length; i++)
                    ws.Cell(row, i).Value = listSetMenuHeader[i - 1];
                int cols = listSetMenuHeader.Length;
                //Item
                row = 2;
                int countIndex = 1;

                foreach (var item in listData)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = item.Symbol;
                    ws.Cell("D" + row).Value = item.IsSelected == true ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Selected") 
                                : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("UnSelected");
                    ws.Cell("E" + row).Value = item.StoreName;

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
