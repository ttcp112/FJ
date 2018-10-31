using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace NuWebNCloud.Shared.Factory.Sandbox.Inventory
{
    public class DiscountFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public DiscountFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<DiscountModels> GetListDiscount(string StoreId = null, string DiscountId = null, List<string> ListOrganizationId = null)
        {
            List<DiscountModels> listData = new List<DiscountModels>();
            try
            {
                DiscountApiModels paraBody = new DiscountApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreId = StoreId;
                paraBody.Id = DiscountId;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetDiscount, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListDiscount"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<DiscountModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listData = listData.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Discount_GetList: " + e);
                return listData;
            }
        }

        public bool InsertOrUpdateDiscount(DiscountModels model, ref string msg)
        {
            try
            {
                DiscountApiModels paraBody = new DiscountApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.Id = model.ID;
                paraBody.StoreId = model.StoreID;

                paraBody.Name = model.Name;
                paraBody.Description = model.Description;
                paraBody.Value = model.Value;
                paraBody.Type = model.Type = (model.BType ? (byte)Commons.EValueType.Currency : (byte)Commons.EValueType.Percent);
                paraBody.IsAllowOpenValue = model.IsAllowOpenValue;
                paraBody.IsApplyTotalBill = model.IsApplyTotalBill;
                paraBody.IsActive = model.IsActive;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditDiscount, null, paraBody);
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
                _logger.Error("Discount_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeleteDiscount(string ID, ref string msg)
        {
            try
            {
                DiscountApiModels paraBody = new DiscountApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.Id = ID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteDiscount, null, paraBody);
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
                _logger.Error("Discount_Delete: " + e);
                return false;
            }
        }


        // IMPORT
        public StatusResponse Import(HttpPostedFileBase excelFile, List<string> lstStore, ref ImportModel importModel, ref string msg)
        {
            StatusResponse Response = new StatusResponse();
            DataTable dt = new DataTable();
            FileInfo[] lstFileImg = new FileInfo[] { };
            Response = ProcessDataImport(ref dt, excelFile, "SBInventoryDiscount.xlsx", out lstFileImg);

            if (!Response.Status)
            {
                msg = Response.MsgError;
                return Response;
            }

            List<DiscountModels> listData = new List<DiscountModels>();

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


                        DiscountModels model = new DiscountModels();

                        model.Index = row[0].ToString();

                        // 1 - Discount Name
                        model.Name = row[1].ToString().Trim().Replace("  ", " ");

                        // 2 - IsActive
                        model.IsActive = GetBoolValue(dt.Columns[2].ColumnName.Replace("#", "."), row[0].ToString(), row[2].ToString());

                        // 3 - Value Type
                        string discountType = row[3].ToString().Trim().Replace("  ", " ");
                        if (discountType == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent"))
                            model.Type = (byte)Commons.EValueType.Percent;
                        else if (discountType == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency"))
                            model.Type = (byte)Commons.EValueType.Currency;
                        else // Null
                            model.Type = (byte)Commons.EValueType.Currency;

                        // 4 - Value
                        //if (!double.TryParse(row[4].ToString(), out doubleVal))
                        //    throw new Exception(string.Format("Data in row #{0} is not valid, {1} must be a number , cannot import this row", row[0].ToString(), dt.Columns[4].ColumnName.Replace("#", ".")));
                        double doubleVal = 0;
                        double.TryParse(row[4].ToString(), out doubleVal);
                        model.Value = doubleVal;

                        string sIsAllowOpenValue = row[5].ToString().Equals("") ? "No" : row[5].ToString();
                        string sIsApplyToTotalBill = row[6].ToString().Equals("") ? "No" : row[6].ToString();
                        // 5 - IsAllowOpenDiscount
                        model.IsAllowOpenValue = GetBoolValue(dt.Columns[5].ColumnName.Replace("#", "."), row[0].ToString(), sIsAllowOpenValue);
                        model.IsApplyTotalBill = GetBoolValue(dt.Columns[6].ColumnName.Replace("#", "."), row[0].ToString(), sIsApplyToTotalBill);
                        // 6 - Remark
                        model.Description = row[7].ToString().Trim().Equals("") ? "$" : row[7].ToString().Trim();

                        model.StoreID = item;

                        if (string.IsNullOrEmpty(model.Name))
                        {
                            flagInsert = false;
                            msgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Name is required");
                        }

                        if (model.Type == (byte)Commons.EValueType.Percent)
                        {
                            if (model.Value < 0 || model.Value > 100)
                            {
                                flagInsert = false;
                                msgError += "<br/>"+ _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0");
                            }
                        }
                        else
                        {
                            if (model.Value < 0)
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value greater than or equal to 0.");
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
                            itemErr.ListErrorMsg.Add("Row:" + model.Index + msgError);
                            importModel.ListImport.Add(itemErr);
                        }
                    }
                    catch (Exception e)
                    {
                        importModel.ListImport.Add(new ImportItem { Name = "Discount", ListFailStoreName = lstStore, ListErrorMsg = new List<string> { e.Message } });
                    }
                }
            }
            Response.Status = true;
            //=====================

            //try
            //{
            DiscountApiModels paraBody = new DiscountApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListDiscount = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportDiscount, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                foreach (ImportResult itemError in listError)
                {
                    itemErr = new ImportItem();
                    itemErr.Name = itemError.Property;
                    itemErr.ListFailStoreName.Add(itemError.StoreName);
                    itemErr.ListErrorMsg.Add("Row: " + itemError.Index + "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error));
                    importModel.ListImport.Add(itemErr);
                }
                if (importModel.ListImport.Count == 0)
                {
                    ImportItem item = new ImportItem();
                    item.Name = "Discount";
                    item.ListSuccessStoreName.Add("Import Discount Successful");
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
                List<DiscountModels> listData = new List<DiscountModels>();

                DiscountApiModels paraBody = new DiscountApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportDiscount, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListDiscount"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<DiscountModels>>(lstContent);
                int cols = 9;
                //Header
                int row = 1;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index");
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Name");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Type");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"); 
                ws.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow Open Discount");
                ws.Cell("G" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Apply to Total Bill");
                ws.Cell("H" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Description");
                ws.Cell("I" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store");
                //Item
                row = 2;
                int countIndex = 1;
                foreach (var item in listData)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");
                    ws.Cell("D" + row).Value = item.Type == (byte)Commons.EValueType.Currency ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Currency") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent");
                    ws.Cell("E" + row).Value = item.Value;
                    ws.Cell("F" + row).Value = item.IsAllowOpenValue ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("G" + row).Value = item.IsApplyTotalBill ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("H" + row).Value = item.Description;
                    ws.Cell("I" + row).Value = item.StoreName;
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
