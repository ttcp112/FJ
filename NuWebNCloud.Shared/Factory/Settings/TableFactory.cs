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
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class TableFactory : BaseFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public TableFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<TableModels> GetListTable(string StoreID = null, string ID = null, List<string> ListOrganizationId = null)
        {
            List<TableModels> listdata = new List<TableModels>();
            try
            {
                TableApiModels paraBody = new TableApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = ID;

                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetTable, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListTable"];
                var lstContent = JsonConvert.SerializeObject(lstZ/*result.RawData*/);
                listdata = JsonConvert.DeserializeObject<List<TableModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                _logger.Error("Table_GetList: " + e);
                return listdata;
            }

        }

        public bool InsertOrUpdateTables(TableModels model, ref string msg)
        {
            try
            {
                TableApiModels paraBody = new TableApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;

                paraBody.Name = model.Name;
                paraBody.ZoneID = model.ZoneID;
                paraBody.Cover = model.Cover;
                paraBody.ViewMode = model.ViewMode;
                paraBody.XPoint = -1;
                paraBody.YPoint = -1;
                paraBody.ZPoint = -1;
                paraBody.IsActive = model.IsActive;
                paraBody.IsShowInReservation = model.IsShowInReservation;
                paraBody.IsTemp = model.IsTemp;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditTable, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        _logger.Error(result.Message);
                        msg = result.Message;
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
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
                _logger.Error("Table_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeleteTables(string ID, ref string msg)
        {
            try
            {
                TableApiModels paraBody = new TableApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteTable, null, paraBody);
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
                _logger.Error("Tables_Delete: " + e);
                return false;
            }
        }

        // IMPORT
        public StatusResponse Import(HttpPostedFileBase excelFile, List<string> lstStore, ref ImportModel importModel, ref string msg)
        {
            StatusResponse Response = new StatusResponse();
            DataTable dt = new DataTable();
            FileInfo[] lstFileImg = new FileInfo[] { };
            Response = ProcessDataImport(ref dt, excelFile, "SBSettingTable.xlsx", out lstFileImg);

            if (!Response.Status)
            {
                msg = Response.MsgError;
                Response.MsgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                return Response;
            }

            List<TableModels> listData = new List<TableModels>();

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

                        double doubleVal = 0;
                        TableModels model = new TableModels();

                        model.Index = row[0].ToString();
                        // 1 - Name
                        model.Name = row[1].ToString().Trim().Replace("  ", " ");

                        // 2 - Maximum Capacity
                        double.TryParse(row[2].ToString(), out doubleVal);
                        model.Cover = (int)doubleVal;

                        // 3 - Zone
                        model.ZoneName = row[3].ToString().Trim().Replace("  ", " ");

                        // 4 - IsActive
                        model.IsActive = GetBoolValue(dt.Columns[4].ColumnName.Replace("#", "."), row[0].ToString(), row[4].ToString());
                         
                        // 5 - Online Reservation
                        string sIsAllowOpenValue = row[5].ToString().Equals("") ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No") : row[5].ToString();
                        model.IsShowInReservation = GetBoolValue(dt.Columns[5].ColumnName.Replace("#", "."), row[0].ToString(), sIsAllowOpenValue);

                        // 6 - Store
                        model.StoreID = item;

                        // 7 - Style
                        string sStyle = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(row[7].ToString());
                        model.ViewMode = sStyle.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Circle.ToString())) ? (byte)Commons.ETableStyle.Circle :
                                         sStyle.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Rectangle.ToString())) ? (byte)Commons.ETableStyle.Rectangle :
                                         sStyle.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Square.ToString())) ? (byte)Commons.ETableStyle.Square : (byte)Commons.ETableStyle.Other;

                        // 8 - XPoint
                        double.TryParse(row[8].ToString(), out doubleVal);
                        model.XPoint = (byte)doubleVal;

                        // 9 - YPoint
                        double.TryParse(row[9].ToString(), out doubleVal);
                        model.YPoint = (byte)doubleVal;
                        //=========
                        string msgItem = "";
                        if (model.Name.Equals(""))
                        {
                            flagInsert = false;
                            msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name is required");
                            msgError += "<br/>" + msgItem;
                        }
                        if (model.Cover < 1)
                        {
                            flagInsert = false;
                            msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value [Maximum Capacity] greater than or equal to 1");
                            msgError += "<br/>" + msgItem;
                        }
                        if (model.ZoneName.Equals(""))
                        {
                            flagInsert = false;
                            msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Zone is required");
                            msgError += "<br/>" + msgItem;
                        }
                        if (row[7].ToString().Equals(""))
                        {
                            flagInsert = false;
                            msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Style is required");
                            msgError += "<br/>" + msgItem;
                        }
                        if (model.XPoint < 0)
                        {
                            flagInsert = false;
                            msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value [Position X] greater than or equal to 0");
                            msgError += "<br/>" + msgItem;
                        }
                        if (model.YPoint < 0)
                        {
                            flagInsert = false;
                            msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a value [Position Y] greater than or equal to 0");
                            msgError += "<br/>" + msgItem;
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
                            itemErr.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + model.Index + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msgError));
                            importModel.ListImport.Add(itemErr);
                        }
                    }
                    catch (Exception e)
                    {
                        importModel.ListImport.Add(new ImportItem { Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Table List"), ListFailStoreName = lstStore, ListErrorMsg = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(e.Message) } });
                    }
                }
            }
            Response.Status = true;
            //=====================

            //try
            //{
            TableApiModels paraBody = new TableApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListTable = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportTable, null, paraBody);
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
                    item.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index + "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error));
                    importModel.ListImport.Add(item);
                }
                if (importModel.ListImport.Count == 0)
                {
                    ImportItem item = new ImportItem();
                    item.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Table List");
                    item.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Table List Successful"));
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
                List<TableModels> listData = new List<TableModels>();

                TableApiModels paraBody = new TableApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportTable, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListTable"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<TableModels>>(lstContent);
                listData = listData.OrderBy(o => o.StoreName).ToList();
                int cols = 10;
                //Header
                int row = 1;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index");
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Maximum Capacity");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Zone");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status");
                ws.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Online Reservation");
                ws.Cell("G" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store");
                ws.Cell("H" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Style");
                ws.Cell("I" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Position X");
                ws.Cell("J" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Position Y");
                //Item
                row = 2;
                int countIndex = 1;
                foreach (var item in listData)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = item.Cover;
                    ws.Cell("D" + row).Value = item.ZoneName;
                    ws.Cell("E" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");
                    ws.Cell("F" + row).Value = item.IsShowInReservation ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("G" + row).Value = item.StoreName;
                    ws.Cell("H" + row).Value = item.ViewMode == (byte)Commons.ETableStyle.Circle ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Circle.ToString()) :
                                               item.ViewMode == (byte)Commons.ETableStyle.Rectangle ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Rectangle.ToString()) :
                                               item.ViewMode == (byte)Commons.ETableStyle.Square ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Square.ToString()) : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.ETableStyle.Other.ToString());
                    ws.Cell("I" + row).Value = item.XPoint;
                    ws.Cell("J" + row).Value = item.YPoint;
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
