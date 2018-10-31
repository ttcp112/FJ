using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class PaymentMethodFactory : BaseFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public PaymentMethodFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<PaymentMethodModels> GetListPaymentMethod(string StoreID = null, string ID = null, List<string> ListOrganizationId = null)
        {
            List<PaymentMethodModels> listdata = new List<PaymentMethodModels>();
            try
            {
                PaymentMethodApiModels paraBody = new PaymentMethodApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;
                paraBody.Mode = 1;
                paraBody.ListOrgID = ListOrganizationId;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetPaymentMethod, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListPaymentMethod"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<PaymentMethodModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listdata = listdata.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();

                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("PaymentMethod_GetList: " + e);
                return listdata;
            }
        }

        public bool InsertOrUpdatePaymentMethod(PaymentMethodModels model, ref string msg)
        {
            try
            {
                PaymentMethodApiModels paraBody = new PaymentMethodApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                //=======
                PaymentMethodModels PayMethodDTO = new PaymentMethodModels
                {
                    ID = model.ID,
                    Name = model.Name,
                    ParentName = model.ParentName,
                    IsActive = model.IsActive,
                    IsHasConfirmCode = model.IsHasConfirmCode,
                    StoreID = model.StoreID,
                    StoreName = model.StoreName,
                    Code = model.Code,
                    NumberOfCopies = model.NumberOfCopies,
                    FixedAmount = model.FixedAmount,
                    Sequence = model.Sequence,
                    IsGiveBackChange = model.IsGiveBackChange,
                    IsAllowKickDrawer = model.IsAllowKickDrawer,
                    IsIncludeOnSale = model.IsIncludeOnSale,
                    IsShowOnPos = model.IsShowOnPos,
                    ListChild = model.ListChild,
                    GLAccountCode = model.GLAccountCode
                };
                paraBody.PayMethodDTO = PayMethodDTO;
                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditPaymentMethod, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                        _logger.Error(result.Message);
                        return false;
                    }
                }
                else
                {
                    msg = result.ToString();
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("PaymentMethod_InsertOrUpdate: " + e);
                msg = e.ToString();
                return false;
            }
        }

        public bool DeletePaymentMethod(string ID, string StoreID, ref string msg)
        {
            try
            {
                PaymentMethodApiModels paraBody = new PaymentMethodApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;
                paraBody.StoreID = StoreID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeletePaymentMethod, null, paraBody);
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
                _logger.Error("PaymentMethod_Delete: " + e);
                return false;
            }
        }

        // IMPORT
        public StatusResponse Import(HttpPostedFileBase excelFile, List<string> lstStore, ref ImportModel importModel, ref string msg)
        {
            StatusResponse Response = new StatusResponse();
            DataTable dt = new DataTable();
            FileInfo[] lstFileImg = new FileInfo[] { };
            Response = ProcessDataImport(ref dt, excelFile, "SBSettingPayment.xlsx", out lstFileImg);

            if (!Response.Status)
            {
                msg = Response.MsgError;
                return Response;
            }

            List<PaymentMethodModels> listData = new List<PaymentMethodModels>();

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

                        PaymentMethodModels model = new PaymentMethodModels();
                        model.Index = row[0].ToString();
                        // 1 - Payment Method Name
                        model.Name = row[1].ToString().Trim().Replace("  ", " ");
                        // 2 - Parent Method Name
                        model.ParentName = row[2].ToString().Trim().Replace("  ", " ");
                        // 3 - IsActive
                        model.IsActive = GetBoolValue(dt.Columns[3].ColumnName.Replace("#", "."), row[0].ToString(), row[3].ToString());

                        // 4 - Confirmation Code
                        string IsHasConfirmCode = row[4].ToString().Equals("") ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("no") : row[4].ToString();
                        model.IsHasConfirmCode = GetBoolValue(dt.Columns[4].ColumnName.Replace("#", "."), row[0].ToString(), IsHasConfirmCode);

                        // 5 - NumberOfCopies 
                        int NumberOfCopies = 0;
                        int.TryParse(row[5].ToString(), out NumberOfCopies);
                        model.NumberOfCopies = NumberOfCopies;

                        //6 -  
                        double FixedAmount = 0;
                        double.TryParse(row[6].ToString(), out FixedAmount);
                        model.FixedAmount = FixedAmount;

                        // 7 - Confirmation Code
                        string IsGiveBackChange = row[7].ToString().Equals("") ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("no") : row[7].ToString();
                        model.IsGiveBackChange = GetBoolValue(dt.Columns[7].ColumnName.Replace("#", "."), row[0].ToString(), IsGiveBackChange);
                        // 8 - Confirmation Code
                        string IsAllowKickDrawer = row[8].ToString().Equals("") ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("no") : row[8].ToString();
                        model.IsAllowKickDrawer = GetBoolValue(dt.Columns[8].ColumnName.Replace("#", "."), row[0].ToString(), IsAllowKickDrawer);
                        // 9 - Confirmation Code
                        string IsIncludeOnSale = row[9].ToString().Equals("") ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("no") : row[9].ToString();
                        model.IsIncludeOnSale = GetBoolValue(dt.Columns[9].ColumnName.Replace("#", "."), row[0].ToString(), IsIncludeOnSale);
                        // 10 - Confirmation Code
                        string IsShowOnPos = row[10].ToString().Equals("") ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("no") : row[10].ToString();
                        model.IsShowOnPos = GetBoolValue(dt.Columns[10].ColumnName.Replace("#", "."), row[0].ToString(), IsShowOnPos);

                        // 11 - Store
                        model.StoreID = item;

                        // 12 - Confirmation Code
                        model.GLAccountCode = row[12].ToString().Trim().Replace("  ", " ");


                        ////Insert List
                        //if (model.ParentName.Equals("")) //Parent
                        //{
                        //    listData.Add(model);
                        //}
                        //else //Child
                        {
                            var parent = listData.Where(x => x.Name.Equals(model.ParentName) && x.StoreID.Equals(item)).FirstOrDefault();
                            if (parent != null)
                            {
                                if (parent.ListChild == null)
                                {
                                    parent.ListChild = new List<PaymentMethodModels>();
                                }
                                parent.ListChild.Add(new PaymentMethodModels
                                {
                                    Name = model.Name,
                                    ParentName = model.ParentName,
                                    IsActive = model.IsActive,
                                    IsHasConfirmCode = model.IsHasConfirmCode,
                                    StoreID = item
                                });
                            }
                        }

                        //===
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
                        importModel.ListImport.Add(new ImportItem
                        {
                            Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Method"),
                            ListFailStoreName = lstStore,
                            ListErrorMsg = new List<string> { e.Message }
                        });
                    }
                }
            }
            Response.Status = true;
            //=====================
            //try
            //{
            listData = listData.Where(x => string.IsNullOrEmpty(x.ParentName)).ToList();
            PaymentMethodApiModels paraBody = new PaymentMethodApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListPaymentMethod = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportPaymentMethod, null, paraBody);
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
                    item.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Method");
                    item.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Payment Method Successful"));
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
                List<PaymentMethodModels> listData = new List<PaymentMethodModels>();

                PaymentMethodApiModels paraBody = new PaymentMethodApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportPaymentMethod, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListPaymentMethod"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<PaymentMethodModels>>(lstContent);

                int cols = 13;
                //Header
                int row = 1;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index");
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Payment Method Name");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Parent Method Name");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Confirmation Code");
                ws.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of Copies");
                ws.Cell("G" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Fixed Amount");
                ws.Cell("H" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Give Back Change");
                ws.Cell("I" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow To Kick Drawer");
                ws.Cell("J" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Include On Sale");
                ws.Cell("K" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Show On Pos");
                ws.Cell("L" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name");
                ws.Cell("M" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("GLAccount Code");
                //Item
                row = 2;
                int countIndex = 1;
                listData = listData.OrderBy(x => x.StoreName).ThenBy(o => o.ParentName).ThenBy(oo => oo.Name).ToList();
                foreach (var item in listData)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = item.ParentName;
                    ws.Cell("D" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");
                    ws.Cell("E" + row).Value = item.IsHasConfirmCode ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("F" + row).Value = item.NumberOfCopies;
                    ws.Cell("G" + row).Value = item.FixedAmount;
                    ws.Cell("H" + row).Value = item.IsGiveBackChange ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("I" + row).Value = item.IsAllowKickDrawer ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("J" + row).Value = item.IsIncludeOnSale ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("K" + row).Value = item.IsShowOnPos ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("L" + row).Value = item.StoreName;
                    ws.Cell("M" + row).Value = item.GLAccountCode;
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
