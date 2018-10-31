using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings.Season;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class SeasonFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public SeasonFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<SeasonModels> GetListSeason(string StoreID = null, string SeasonId = null, List<string> ListOrganizationId = null)
        {
            List<SeasonModels> listData = new List<SeasonModels>();
            try
            {
                SeasonApiModels paraBody = new SeasonApiModels();

                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.IsInventory = false;

                paraBody.StoreID = StoreID;
                paraBody.ID = SeasonId;

                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetSeason, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListSeason"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<SeasonModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Season_GetList: " + e);
                return listData;
            }
        }

        public bool InsertOrUpdateSeason(SeasonModels model, ref string msg)
        {
            try
            {
                SeasonApiModels paraBody = new SeasonApiModels();

                SeasonModels SeasonDTO = new SeasonModels();
                SeasonDTO.ID = model.ID;
                SeasonDTO.Name = model.Name;

                SeasonDTO.StartDate = new DateTime(model.StartDate.Year, model.StartDate.Month, model.StartDate.Day,
                12, 0, 0);

                SeasonDTO.EndDate = new DateTime(model.EndDate.Year, model.EndDate.Month, model.EndDate.Day,
                12, 0, 0);

                if (model.Unlimited)
                {
                    SeasonDTO.StartTime = model.StartTime;
                    SeasonDTO.EndTime = model.EndTime;
                }
                else
                {
                    SeasonDTO.StartTime = model.StartDate.Add(model.TStartTime);
                    SeasonDTO.EndTime = model.EndDate.Add(model.TEndTime);
                }

                SeasonDTO.RepeatType = model.RepeatType;
                SeasonDTO.ListDay = model.ListDay;
                SeasonDTO.Unlimited = model.Unlimited;
                SeasonDTO.StoreID = model.StoreID;
                //---========
                paraBody.SeasonDTO = SeasonDTO;

                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.StoreID = model.StoreID;
                paraBody.ID = model.ID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditSeason, null, paraBody);
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
                _logger.Error("Season_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeleteSeason(string ID, ref string msg)
        {
            try
            {
                SeasonApiModels paraBody = new SeasonApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteSeason, null, paraBody);
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
                _logger.Error("Season_Delete: " + e);
                return false;
            }
        }

        //public bool DeleteSeason(string ID)
        //{
        //    try
        //    {
        //        SeasonApiModels paraBody = new SeasonApiModels();
        //        paraBody.AppKey = Commons.AppKey;
        //        paraBody.AppSecret = Commons.AppSecret;
        //        paraBody.CreatedUser = Commons.CreateUser;
        //        paraBody.RegisterToken = new RegisterTokenModels();

        //        paraBody.ID = ID;

        //        //====================
        //        var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteSeason, null, paraBody);
        //        if (result != null)
        //        {
        //            if (result.Success)
        //                return true;
        //            else
        //            {
        //                _logger.Error(result.Message);
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            _logger.Error(result);
        //            return false;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error("Season_Delete: " + e);
        //        return false;
        //    }
        //}

        // IMPORT
        public StatusResponse Import(HttpPostedFileBase excelFile, List<string> lstStore, ref ImportModel importModel, ref string msg)
        {
            StatusResponse Response = new StatusResponse();
            DataTable dt = new DataTable();
            FileInfo[] lstFileImg = new FileInfo[] { };
            Response = ProcessDataImport(ref dt, excelFile, "SBSettingSeason.xlsx", out lstFileImg);

            if (!Response.Status)
            {
                msg = Response.MsgError;
                Response.MsgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msg);
                return Response;
            }

            ImportItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            List<SeasonModels> listData = new List<SeasonModels>();
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

                        string msgItem = "";
                        DateTime StartDate = DateTimeHelper.GetDateImport(row[2].ToString(), ref msgItem);
                        DateTime EndDate = DateTimeHelper.GetDateImport(row[3].ToString(), ref msgItem);

                        DateTime StartTime = row[4].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("unlimited")) ? Commons._MinDate : DateTimeHelper.GetTimeImport(row[4].ToString(), ref msgItem);
                        DateTime EndTime = row[5].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("unlimited")) ? Commons._MinDate : DateTimeHelper.GetTimeImport(row[5].ToString(), ref msgItem);

                        if (!msgItem.Equals(""))
                        {
                            flagInsert = false;
                            msgError = msgItem;
                        }

                        SeasonModels model = new SeasonModels();
                        model.Index = row[0].ToString();
                        // 1 - Discount Name
                        model.Name = row[1].ToString().Trim().Replace("  ", " ");

                        model.StartDate = row[2].ToString().Equals("") ? Commons._ExpiredDate : StartDate;
                        model.EndDate = row[3].ToString().Equals("") ? Commons._ExpiredDate : EndDate;

                        model.StartTime = StartTime;
                        model.EndTime = EndTime;

                        model.RepeatType = row[6].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.RPT_DayOfWeek.ToLower())) ? (byte)Commons.ERepeatType.DayOfWeek :
                                   row[6].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.RPT_DayOfMonth.ToLower())) ? (byte)Commons.ERepeatType.DayOfMonth : 0;

                        List<int> lstDays = new List<int>();
                        if (!string.IsNullOrEmpty(row[7].ToString()))
                        {
                            string[] arrInt = row[7].ToString().Split('-');
                            for (int i = 0; i < arrInt.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(arrInt[i]))
                                {
                                    lstDays.Add(int.Parse(arrInt[i]));
                                }
                            }
                        }
                        model.ListDay = lstDays;
                        model.StoreID = item;

                        if (model.StartDate > model.EndDate)
                        {
                            flagInsert = false;
                            msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Start Date must be less than To Date.");
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
                        //Insert List
                        //listData.Add(model);
                    }
                    catch (Exception e)
                    {
                        importModel.ListImport.Add(new ImportItem { Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season"), ListFailStoreName = lstStore, ListErrorMsg = new List<string> { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(e.Message) } });
                    }
                }
            }
            Response.Status = true;
            //=====================

            //try
            //{
            SeasonApiModels paraBody = new SeasonApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListSeason = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportSeason, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                foreach (var itemError in listError)
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
                    item.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season");
                    item.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Season Successful"));
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
                List<SeasonModels> listData = new List<SeasonModels>();

                SeasonApiModels paraBody = new SeasonApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportSeason, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListSeason"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<SeasonModels>>(lstContent);
                int cols = 9;
                //Header
                int row = 1;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index");
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Season Name");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Start Date");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("End Date");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("StartTime");
                ws.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("EndTime");
                ws.Cell("G" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Day Range");
                ws.Cell("H" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Days");
                ws.Cell("I" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name");
                //Item
                row = 2;
                int countIndex = 1;
                foreach (var item in listData)
                {
                    string days = "";
                    if (item.ListDay != null)
                    {
                        foreach (var day in item.ListDay)
                        {
                            days += day + "-";
                        }
                    }
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = "'" + item.StartDate.ToString("dd/MM/yyy");
                    ws.Cell("D" + row).Value = "'" + item.EndDate.ToString("dd/MM/yyy");
                    //============
                    if (item.StartTime.Value.Date == Commons._UnlimitedDate.Date || item.EndTime.Value.Date == Commons._UnlimitedDate.Date)
                    {
                        ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited");
                        ws.Cell("F" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited");
                    }
                    else
                    {
                        ws.Cell("E" + row).Value = "'" + item.StartTime.Value.ToLocalTime().ToString("HH:mm");
                        ws.Cell("F" + row).Value = "'" + item.EndTime.Value.ToLocalTime().ToString("HH:mm");
                    }
                    //=============
                    ws.Cell("G" + row).Value = item.RepeatType == (byte)Commons.ERepeatType.DayOfWeek ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.RPT_DayOfWeek) :
                                                 item.RepeatType == (byte)Commons.ERepeatType.DayOfMonth ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.RPT_DayOfMonth) : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Other");
                    ws.Cell("H" + row).Value = days;
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
