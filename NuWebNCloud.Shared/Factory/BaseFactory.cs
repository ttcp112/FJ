using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using System.Data;
using System.Web;
using System.IO;
using NuWebNCloud.Shared.Utilities;
using ClosedXML.Excel;
using System.Reflection;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using System.Text.RegularExpressions;
using NuWebNCloud.Shared.Models.Reports;
using System.Data.OleDb;
using NuWebNCloud.Shared.Models.Api;
using System.Configuration;
using NuWebNCloud.Shared.Models.Settings.Location;

namespace NuWebNCloud.Shared.Factory
{
    public class BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public NuWebContext AddToContext(NuWebContext context, BaseEntity entity, int count, int commitCount, bool recreateContext)
        {
            context.Entry(entity).State = EntityState.Added;
            if (count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new NuWebContext();
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }

            return context;
        }

        public async void InsertTrackingLog(string tableName, string info, string storeId, bool result)
        {
            try
            {
                using (var cxt = new NuWebContext())
                {
                    G_TrackingLog trackingLog = new G_TrackingLog();
                    trackingLog.Id = Guid.NewGuid().ToString();
                    trackingLog.TableName = tableName;
                    trackingLog.StoreId = storeId;
                    trackingLog.CreatedDate = DateTime.Now;
                    trackingLog.JsonContent = info;
                    trackingLog.IsDone = result;

                    cxt.G_TrackingLog.Add(trackingLog);
                    cxt.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

        }

        public async void InsertIngredientTrackLog(string code)
        {
            try
            {
                using (var cxt = new NuWebContext())
                {
                    I_IngredientTrackLog trackingLog = new I_IngredientTrackLog();
                    trackingLog.Id = Guid.NewGuid().ToString();
                    trackingLog.Code = code;
                    trackingLog.CreatedDate = DateTime.Now;

                    cxt.I_IngredientTrackLog.Add(trackingLog);
                    cxt.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

        }

        public async void InsertUsageXeroTrackLog(DateTime toDate, string storeId)
        {
            try
            {
                using (var cxt = new NuWebContext())
                {
                    I_UsageManagementXeroTrackLog trackingLog = new I_UsageManagementXeroTrackLog();
                    trackingLog.Id = Guid.NewGuid().ToString();
                    trackingLog.ToDate = toDate;
                    trackingLog.StoreId = storeId;
                    trackingLog.CreatedDate = DateTime.Now;

                    cxt.I_UsageManagementXeroTrackLog.Add(trackingLog);
                    cxt.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

        }

        public async void DeleteTrackingLog()
        {
            using (var cxt = new NuWebContext())
            {
                try
                {
                    int month = DateTime.Now.AddMonths(-1).Month, year = DateTime.Now.AddMonths(-1).Year;
                    var dataMonthBefore = cxt.G_TrackingLog.Where(ww => ww.CreatedDate.Month == month && ww.CreatedDate.Year == year).ToList();
                    if (dataMonthBefore != null && dataMonthBefore.Any())
                    {
                        cxt.G_TrackingLog.RemoveRange(dataMonthBefore);
                        cxt.SaveChanges();
                    }
                }
                catch (Exception ex)
                {

                    _logger.Error(ex);
                }

            }
        }

        public List<BusinessDayDisplayModels> GetBusinessDays_Old(DateTime dFrom, DateTime dTo, List<string> lstStoreId, int mode)
        {
            var result = new List<BusinessDayDisplayModels>();
            using (var cxt = new NuWebContext())
            {
                var query = cxt.G_BusinessDay.Where(ww => ((ww.StartedOn >= dFrom
                && ww.StartedOn <= dTo)
                || (DbFunctions.TruncateTime(ww.StartedOn) <= DbFunctions.TruncateTime(dFrom) && DbFunctions.TruncateTime(ww.ClosedOn) >= DbFunctions.TruncateTime(dTo))
                || (DbFunctions.TruncateTime(ww.ClosedOn) == DbFunctions.TruncateTime(dFrom)))
                  && lstStoreId.Contains(ww.StoreId) && ww.Mode == mode)
                   .ToList();
                foreach (var item in query)
                {
                    var obj = new BusinessDayDisplayModels();
                    obj.DateFrom = item.StartedOn;
                    obj.StoreId = item.StoreId;
                    obj.Id = item.Id;
                    if (item.ClosedOn == Commons.MinDate)
                    {
                        obj.DateTo = new DateTime(item.StartedOn.Year, item.StartedOn.Month, item.StartedOn.Day, 23, 59, 59);
                        obj.DateDisplay = item.StartedOn.ToString("dd/MM/yyyy HH:mm") + " - ";
                    }
                    else
                    {
                        obj.DateTo = item.ClosedOn;
                        obj.DateDisplay = item.StartedOn.ToString("dd/MM/yyyy HH:mm") + " - " + item.ClosedOn.ToString("dd/MM/yyyy HH:mm");
                    }
                    result.Add(obj);
                }
            }
            result = result.OrderBy(oo => oo.DateFrom).ToList();
            return result;
        }
        public List<BusinessDayDisplayModels> GetBusinessDays(DateTime dFrom, DateTime dTo, List<string> lstStoreId, int mode)
        {
            var result = new List<BusinessDayDisplayModels>();
            using (var cxt = new NuWebContext())
            {
                //var query = cxt.G_BusinessDay.Where(ww =>
                //   ww.StartedOn >= dFrom && ww.StartedOn <= dTo
                //    && lstStoreId.Contains(ww.StoreId) && ww.Mode == mode)
                //   .ToList();
                var query = (from b in cxt.G_BusinessDay.AsNoTracking()
                             where b.StartedOn >= dFrom && b.StartedOn <= dTo
                    && lstStoreId.Contains(b.StoreId) && b.Mode == mode
                             select new BusinessDayModels()
                             {
                                 StartedOn = b.StartedOn,
                                 StoreId = b.StoreId,
                                 ClosedOn = b.ClosedOn,
                                 Id = b.Id
                             }).ToList();
                foreach (var item in query)
                {
                    var obj = new BusinessDayDisplayModels();
                    obj.DateFrom = item.StartedOn;
                    obj.StoreId = item.StoreId;
                    obj.Id = item.Id;
                    if (item.ClosedOn == Commons.MinDate)
                    {
                        obj.DateTo = new DateTime(item.StartedOn.Year, item.StartedOn.Month, item.StartedOn.Day, 23, 59, 59);
                        obj.DateDisplay = item.StartedOn.ToString("dd/MM/yyyy HH:mm") + " - ";
                    }
                    else
                    {
                        obj.DateTo = item.ClosedOn;
                        obj.DateDisplay = item.StartedOn.ToString("dd/MM/yyyy HH:mm") + " - " + item.ClosedOn.ToString("dd/MM/yyyy HH:mm");
                    }
                    result.Add(obj);
                }
            }
            result = result.OrderBy(oo => oo.DateFrom).ToList();
            return result;
        }
        public List<BusinessDayDisplayModels> GetBusinessDays(DateTime dFrom, DateTime dTo, string storeId, int mode)
        {
            var result = new List<BusinessDayDisplayModels>();
            using (var cxt = new NuWebContext())
            {
                var query = cxt.G_BusinessDay.Where(ww => ww.StartedOn >= dFrom
                && ww.StartedOn <= dTo && ww.StoreId == storeId && ww.Mode == mode)
                   .ToList();
                foreach (var item in query)
                {
                    var obj = new BusinessDayDisplayModels();
                    obj.DateFrom = item.StartedOn;
                    obj.StoreId = item.StoreId;
                    obj.Id = item.Id;
                    if (item.ClosedOn == Commons.MinDate)
                    {
                        obj.DateTo = new DateTime(item.StartedOn.Year, item.StartedOn.Month, item.StartedOn.Day, 23, 59, 59);
                        obj.DateDisplay = item.StartedOn.ToString("dd/MM/yyyy HH:mm") + " - ";
                    }
                    else
                    {
                        obj.DateTo = item.ClosedOn;
                        obj.DateDisplay = item.StartedOn.ToString("dd/MM/yyyy HH:mm") + " - " + item.ClosedOn.ToString("dd/MM/yyyy HH:mm");
                    }
                    result.Add(obj);
                }
            }
            return result;
        }

        /**/
        public StatusResponse ProcessDataImport(ref DataTable dt, HttpPostedFileBase excelFile, string templateExcelName, out FileInfo[] lstFileImg, HttpPostedFileBase imgZipFile = null)
        {
            var Response = new StatusResponse();
            lstFileImg = null;
            string excelPath = "";
            string imageZipPath = "";

            if (!CommonHelper.SaveFileExcelToServer(excelFile, out excelPath))
            {
                Response.Status = false;
                Response.MsgError = CommonHelper._InnerException;
                return Response;
            }

            if (imgZipFile != null)
            {
                lstFileImg = CommonHelper.GetListFileFromZip(imgZipFile, out imageZipPath);
                if (lstFileImg == null)
                {
                    Response.Status = false;
                    Response.MsgError = CommonHelper._InnerException;
                    return Response;
                }
            }

            dt = CommonHelper.GetDataTableFromExcelFile(excelPath, templateExcelName);
            if (dt == null)
            {
                Response.Status = false;
                Response.MsgError = CommonHelper._InnerException;
                return Response;
            }

            Response.Status = true;
            return Response;
        }

        public static bool GetBoolValue(string columnName, string rowCount, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
                //throw new Exception(string.Format("Data in row #{0} is not valid, {1} cannot be null, cannot import this row", rowCount, columnName));
            }
            //value = value.Trim().ToLower();
            value = UppercaseFirst(value);

            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("True"))
                return true;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("False"))
                return false;
            if (value == "1")
                return true;
            if (value == "0")
                return false;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes"))
                return true;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No"))
                return false;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active"))
                return true;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive"))
                return false;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Male"))
                return true;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Female"))
                return false;

            //throw new Exception(string.Format("Data in row #{0} is not valid, {1} must be a boolean value, cannot import this row", rowCount, columnName));
            throw new Exception(string.Format("{0} #{1}{2} #{3}{4} ", _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Data in row"), rowCount, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("is not valid"),
                columnName, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("must be a boolean value, cannot import this row")));

        }

        static string UppercaseFirst(string s)
        {
            try
            {
                if (string.IsNullOrEmpty(s))
                {
                    return string.Empty;
                }
                char[] a = s.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                return new string(a);
            }
            catch
            {
            }
            return s;
        }
        public static bool GetBoolValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            //value = value.Trim().ToLower();
            value = value.Trim();
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("True") /*"true"*/)
                return true;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("False") /*"false"*/)
                return false;
            if (value == "1")
                return true;
            if (value == "0")
                return false;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes") /*"yes"*/)
                return true;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No") /*"no"*/)
                return false;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") /*"active"*/)
                return true;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive") /*"inactive"*/)
                return false;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Male")/*"male"*/)
                return true;
            if (value == _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Female") /*"female"*/)
                return false;
            throw new Exception();
        }

        public static void FormatExcelExport(IXLWorksheet ws, int lastRow, int lastCol)
        {
            // Format Excel
            ws.Range(1, 1, 1, lastCol).Style.Font.SetBold(true);
            ws.Range(1, 1, lastRow, lastCol).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            ws.Range(1, 1, lastRow, lastCol).Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

            ws.Range(1, 1, lastRow - 1, lastCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, lastRow - 1, lastCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Columns(1, lastCol).AdjustToContents();
        }

        public List<T> GetListObject<T>(DataTable dt) where T : new()
        {
            List<T> lst = new List<T>();
            for (int rowCount = 0; rowCount < dt.Rows.Count; rowCount++)
            {
                T obj = new T();
                InforImportModel model = new InforImportModel();
                DataRow cells = dt.Rows[rowCount];
                PropertyInfo[] props = obj.GetType().GetProperties();

                // Check Row Nulls
                string text = "";
                for (int i = 0; i < cells.Table.Columns.Count; i++)
                    text += cells[i].ToString();

                if (string.IsNullOrEmpty(text))
                    continue;

                for (int cellCount = 0; cellCount < cells.Table.Columns.Count; cellCount++)
                {
                    try
                    {
                        MapProperty(ref obj, props[cellCount].Name, cells[cellCount].ToString());
                    }
                    catch
                    {
                        model.Errors.Add(cells.Table.Columns[cellCount].ColumnName + " is not valid");
                        model.IsValidRow = false;
                        continue;
                    }
                }
                obj.GetType().GetProperty("Infor").SetValue(obj, model, null);
                obj.GetType().GetProperty("RowCount").SetValue(obj, (rowCount + 1), null);
                lst.Add(obj);
            }
            return lst;
        }

        protected void MapProperty<T>(ref T obj, string propName, string value) where T : new()
        {
            PropertyInfo prop = obj.GetType().GetProperty(propName);
            string propType = prop.PropertyType.ToString();
            try
            {
                switch (propType)
                {
                    case "System.Boolean":
                        prop.SetValue(obj, GetBoolValue(value), null);
                        break;
                    case "System.Double":
                        prop.SetValue(obj, GetDoubleValue(value), null);
                        break;
                    case "System.Decimal":
                    case "System.Nullable`1[System.Decimal]":
                        prop.SetValue(obj, GetDecimalValue(value), null);
                        break;
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Int16":
                        prop.SetValue(obj, GetIntValue(value), null);
                        break;
                    case "System.DateTime":
                        prop.SetValue(obj, GetDateTimeValue(value), null);
                        break;
                    default:
                        prop.SetValue(obj, Regex.Replace(value, @"\s+", " "), null);
                        break;
                }
            }
            catch
            {
                throw;
            }
        }

        public static double GetDoubleValue(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return 0;
                return double.Parse(value);
            }
            catch
            {
                throw;
            }
        }

        public static decimal GetDecimalValue(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return 0;
                return decimal.Parse(value);
            }
            catch
            {
                throw;
            }
        }

        public static int GetIntValue(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return 0;
                return int.Parse(value);
            }
            catch
            {
                throw;
            }
        }
        public static DateTime GetDateTimeValue(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || value.Trim().ToLower() == "never")
                    value = CommonHelper._NeverDate;
                return DateTime.Parse(value);
            }
            catch
            {
                throw;
            }
        }

        /*Factory Extension*/
        /*===========Set Menu*/
        public DataTable ReadExcelFile(string path, string sheetName)
        {
            DataTable dt = new DataTable();
            using (OleDbConnection conn = new OleDbConnection())
            {
                //conn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";" + "Extended Properties='Excel 12.0 Xml;HDR=YES;'";
                conn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";" + "Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1;MAXSCANROWS=0'";
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandText = "SELECT * FROM [" + sheetName + "$]";
                cmd.Connection = conn;
                OleDbDataAdapter da = new OleDbDataAdapter();
                da.SelectCommand = cmd;
                try
                {
                    conn.Open();
                    da.Fill(dt);
                }
                finally
                {
                    da.Dispose();
                    cmd.Dispose();
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                    conn.Dispose();
                }
                return dt;
            }
        }

        public List<RFilterChooseExtBaseModel> GetAllPaymentForReport(CategoryApiRequestModel request)
        {
            var lstData = new List<RFilterChooseExtBaseModel>();
            try
            {
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetPaymentMethodForWeb, null, request);
                if (result.Success)
                {
                    dynamic data = result.Data;
                    var ListCate = data["ListCategories"];

                    var lstContent = JsonConvert.SerializeObject(ListCate);
                    lstData = JsonConvert.DeserializeObject<List<RFilterChooseExtBaseModel>>(lstContent);

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return lstData;
        }
        public List<RFilterChooseExtBaseModel> GetAllPaymentForMerchantExtendReport(string hostUrl, CategoryApiRequestModel request)
        {
            var lstData = new List<RFilterChooseExtBaseModel>();
            try
            {
                var result = (ResponseApiModels)ApiResponse.PostWithoutHostConfig<ResponseApiModels>(hostUrl + "/" + Commons.GetPaymentMethodForWeb, null, request);
                if (result.Success)
                {
                    dynamic data = result.Data;
                    var ListCate = data["ListCategories"];

                    var lstContent = JsonConvert.SerializeObject(ListCate);
                    lstData = JsonConvert.DeserializeObject<List<RFilterChooseExtBaseModel>>(lstContent);

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return lstData;
        }

        public bool IsTaxInclude(string storeId)
        {
            TaxFactory factory = new TaxFactory();
            var taxes = factory.GetDetailTaxForStore(storeId);
            if (taxes == (int)Commons.ETax.AddOn)
            {
                return false;
            }
            return true;
        }

        public string GetCountryCode()
        {
            string coutryCode = string.Empty;
            try
            {
                string apiLink = ConfigurationManager.AppSettings["RestCountriesCodeApi"] ?? "";
                string data = (string)ApiResponse.GetWithoutHostConfigReturnContent<string>(apiLink, null, null);
                if (!string.IsNullOrEmpty(data))
                {
                    coutryCode = data.Replace("<br>", "/").Split('/')[1];
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetCountryCode : ", ex);
            }
            return coutryCode;
        }

        public List<CountryModels> GetListCountry()
        {
            List<CountryModels> listData = new List<CountryModels>();
            //check session
            if (HttpContext.Current.Session["CountriesLibSession"] != null)
                listData = (List<CountryModels>)System.Web.HttpContext.Current.Session["CountriesLibSession"];
            else
            {
                string APICountry = ConfigurationManager.AppSettings["RestCountriesApi"] ?? "";
                listData = (List<CountryModels>)ApiResponse.GetWithoutHostConfig<List<CountryModels>>(APICountry, null, null);
                //write to session
                HttpContext.Current.Session.Add("CountriesLibSession", listData);
            }
            if (listData == null)
                listData = new List<CountryModels>();
            return listData;
        }
    }
}
