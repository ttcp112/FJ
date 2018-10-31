using NuWebNCloud.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

// Updated 08292017
using NuWebNCloud.Shared.Models.Api;
using NLog;
using Newtonsoft.Json;
using System.Configuration;
using System.Dynamic;
using System.Globalization;

namespace NuWebNCloud.Shared.Utilities
{
    public class CommonHelper
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public static string GetSHA512(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            SHA512Managed hashString = new SHA512Managed();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        public static string _InnerException { get; set; }
        public static string _SheetName = "Sheet1";
        public static string _NeverDate = "30/12/9999";

        /* Generate the name of export file */
        public static string GetExportFileName(string name)
        {
            return string.Format("Export_{0}_{1}", name, DateTime.Now.ToString("ddMMyy"));
        }

        /*Resize image */
        public static Bitmap ResizeImage(Image image, int width = 150, int height = 150)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        /* Convert Image -> byte[] -> base64String */
        public static string ImageToBase64(Image image)
        {
            //ImageFormat format = new ImageFormat(Guid.NewGuid());
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    // Convert Image to byte[]
                    image.Save(ms, ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    ms.Close();
                    return base64String;

                }
                catch (Exception)
                {
                    ms.Close();
                    return "";
                }

            }
        }

        /* Extract ZipFile */
        public static void ExtractZipFile(string filePath, string serverZipExtractPath)
        {
            string zipToUnpack = filePath;
            string unpackDirectory = serverZipExtractPath;
            using (Ionic.Zip.ZipFile zip1 = Ionic.Zip.ZipFile.Read(zipToUnpack))
            {
                foreach (Ionic.Zip.ZipEntry e in zip1)
                {
                    e.Extract(unpackDirectory, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                }
            }

        }

        public static bool IsDirectoryEmpty(string path)
        {
            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }

        public static FileInfo[] GetListFileFromZip(HttpPostedFileBase zipFile, out string imageZipPath)
        {
            FileInfo[] listFiles = new FileInfo[] { }; //list image file in folder after extract
            imageZipPath = GetFilePath(zipFile);
            try
            {

                var _ExtractFolderPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads") + "/ExtractFolder";
                //upload file to server
                if (System.IO.File.Exists(imageZipPath))
                    System.IO.File.Delete(imageZipPath);
                zipFile.SaveAs(imageZipPath);

                //extract file
                //ExtractZipFile(filePath, _serverZipExtractPath);
                ExtractZipFile(imageZipPath, _ExtractFolderPath);
                //delete zip file after extract
                if (System.IO.File.Exists(imageZipPath))
                    System.IO.File.Delete(imageZipPath);

                if (!IsDirectoryEmpty(_ExtractFolderPath))
                {

                    string[] extensions = new[] { ".jpg", ".jpeg", ".png" };
                    DirectoryInfo dInfo = new DirectoryInfo(_ExtractFolderPath); //Assuming Test is your Folder
                    //Getting Text files
                    listFiles =
                        dInfo.EnumerateFiles()
                             .Where(f => extensions.Contains(f.Extension.ToLower()))
                             .ToArray();
                }
                return listFiles; ;
            }
            catch (Exception e)
            {
                _InnerException = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Error occur when unzip folder image:") + e.Message;
                return null;
            }
        }

        public static string GetFilePath(HttpPostedFileBase file)
        {
            var _UploadPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads");
            return string.Format("{0}/{1}", _UploadPath, Path.GetFileName(file.FileName));
        }

        public static bool SaveFileExcelToServer(HttpPostedFileBase excelFile, out string filePath)
        {
            try
            {
                filePath = GetFilePath(excelFile);

                //upload file to server
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                excelFile.SaveAs(filePath);

                return true;
            }
            catch (Exception e)
            {
                _InnerException = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Error occur when save file excel to server:") + e.Message;
                filePath = "";
                return false;
            }

        }

        public static bool DeleteFileFromServer(string path, bool isImageZipFile = false)
        {
            try
            {
                //delete file excel after insert to database
                if (!isImageZipFile)
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                else
                {
                    var _ExtractFolderPath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads") + "/ExtractFolder";
                    //delete folder extract after get file.
                    if (string.IsNullOrEmpty(path))
                        path = _ExtractFolderPath;
                    if (System.IO.Directory.Exists(path))
                        System.IO.Directory.Delete(path, true);
                }

                return true;
            }
            catch (Exception e)
            {
                _InnerException = e.Message;
                return false;
            }
        }

        public static DataTable GetDataTableFromExcelFile(string excelPath, string templateExcelName)
        {
            OleDbConnection conn = null;
            OleDbDataAdapter da = null;
            OleDbDataAdapter tplDa = null;
            DataTable dt = null;
            DataTable tplDt = null;
            try
            {
                string connectionString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=YES; IMEX=1;'; Persist Security Info=False", excelPath);
                conn = new OleDbConnection(connectionString);
                conn.Open();

                string query = "SELECT * FROM [" + _SheetName + "$]";

                da = new OleDbDataAdapter(query, conn);
                dt = new DataTable();
                da.Fill(dt);
                conn.Close();
                // Template
                var _TemplateExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate");
                string templateExcelPath = string.Format("{0}/{1}", _TemplateExcelPath, templateExcelName);

                if (!System.IO.File.Exists(templateExcelPath))
                {
                    //_InnerException = "Not have template excel file. Please add template excel file to procject first";
                    _InnerException = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Not have template excel file. Please add template excel file to procject first");
                    return null;
                }

                string tplConnectionString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=YES; IMEX=1;'; Persist Security Info=False", templateExcelPath);
                conn = new OleDbConnection(tplConnectionString);
                conn.Open();

                tplDa = new OleDbDataAdapter(query, conn);
                tplDt = new DataTable();
                tplDa.Fill(tplDt);

                if (dt.Columns.Count != tplDt.Columns.Count)
                {
                    _InnerException = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);// "Your excel file was not match template excel file.";
                    return null;
                }

                //List<string> listNames = new List<string>() { "store", "group/area/store", "store/area/group", "store name", "storename", "store/area/group name" };
                List<string> listNames = new List<string>() { _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("group/area/store"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("store/area/group"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("storename"), _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store/Area/Group Name") };

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    //string tempColumnName = tplDt.Columns[i].ColumnName.Trim().ToLower();
                    string tempColumnName = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(tplDt.Columns[i].ColumnName.Trim());
                    if (listNames.Contains(tempColumnName))
                        continue;
                    if (dt.Columns[i].ColumnName.Trim() != _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(tplDt.Columns[i].ColumnName.Trim()))
                    {
                        _InnerException = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);//"Your excel file was not match template excel file.";
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                _InnerException = e.Message;
            }
            finally
            {
                if (tplDt != null)
                    tplDt.Dispose();
                if (dt != null)
                    dt.Dispose();
                if (tplDa != null)
                    tplDa.Dispose();
                if (da != null)
                    da.Dispose();
                if (conn != null)
                    conn.Close();
            }

            return dt;
        }

        //public static bool CompareTemplateWithFileExcel(string templatePath, string excelPath)
        //{
        //    OleDbConnection conn = null;
        //    DataTable dt = null;
        //    DataSet ds = null;
        //    string connectionString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=YES; IMEX=1;'; Persist Security Info=False", excelPath);
        //    try
        //    {

        //        return true;
        //    }
        //    catch (Exception e)
        //    {

        //        return false;
        //    }
        //}

        /* Convert FileInfo Object To Image was resized, then to base64String for saving to Database  */
        public static string ConvertFileInfoToBase64String(FileInfo fileInfo)
        {
            try
            {
                //convert to image
                byte[] bytes = new byte[fileInfo.Length];
                Stream stream = fileInfo.OpenRead();
                Image imgage = Image.FromStream(stream, true, true);
                Bitmap bitmap = ResizeImage(imgage);
                stream.Close();
                return ImageToBase64(bitmap);
            }
            catch (Exception e)
            {
                string error = e.ToString();
                //logger.Error("Convert FileInfo To Byte Array Fail. Please check in Data Processing Function!" + e.Message);
                return "";
            }
        }

        /* Trim spaces */
        public static string Trim(string input)
        {
            return Regex.Replace(input, @"\s+", " ");
        }

        public static bool IsValidEmail(string email)
        {
            Regex regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            if (regex.IsMatch(email))
            {
                return true;
            }
            return false;
        }
        public static bool IsNumber(string snumber)
        {
            Regex regex = new Regex("^[0-9]+$");
            if (regex.IsMatch(snumber))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 1: I_Purchase_Order
        /// 2: I_ReceiptNote
        /// </summary>
        /// <param name="tableCode"></param>
        /// <returns></returns>
        public static string GetGenNo(Commons.ETableZipCode tableCode, string storeId)
        {
            string code = string.Empty;
            switch (tableCode)
            {
                case Commons.ETableZipCode.PurchaseOrder:
                    using (var db = new NuWebContext())
                    {
                        var nuRecords = db.I_Purchase_Order.Count(ww => DbFunctions.TruncateTime(ww.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) && ww.StoreId == storeId);
                        nuRecords++;
                        code = "PO-" + DateTime.Now.ToString("ddMMyyyy") + "-" + nuRecords.ToString().PadLeft(4, '0');
                    }
                    break;

                case Commons.ETableZipCode.WorkOrder:
                    using (var db = new NuWebContext())
                    {
                        var nuRecords = db.I_Work_Order.Count(ww => DbFunctions.TruncateTime(ww.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) && ww.StoreId == storeId);
                        nuRecords++;
                        code = "WO-" + DateTime.Now.ToString("ddMMyyyy") + "-" + nuRecords.ToString().PadLeft(4, '0');
                    }
                    break;

                case Commons.ETableZipCode.ReceiptNote:
                    using (var db = new NuWebContext())
                    {
                        var nuRecords = db.I_ReceiptNote.Count(ww => DbFunctions.TruncateTime(ww.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) && ww.StoreId == storeId);
                        nuRecords++;
                        code = "RN-" + DateTime.Now.ToString("ddMMyyyy") + "-" + nuRecords.ToString().PadLeft(4, '0');
                    }
                    break;

                case Commons.ETableZipCode.ReturnNote:
                    using (var db = new NuWebContext())
                    {
                        var nuRecords = (from ret in db.I_Return_Note
                                         join r in db.I_ReceiptNote on ret.ReceiptNoteId equals r.Id
                                         where DbFunctions.TruncateTime(ret.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) && r.StoreId == storeId
                                         select ret).Count();
                        nuRecords++;
                        code = "RT-" + DateTime.Now.ToString("ddMMyyyy") + "-" + nuRecords.ToString().PadLeft(4, '0');
                    }
                    break;

                case Commons.ETableZipCode.StockTransfer:
                    using (var db = new NuWebContext())
                    {
                        var nuRecords = db.I_Stock_Transfer.Count(ww => DbFunctions.TruncateTime(ww.RequestDate) == DbFunctions.TruncateTime(DateTime.Now) && ww.IssueStoreId == storeId);
                        nuRecords++;
                        code = "ST-" + DateTime.Now.ToString("ddMMyyyy") + "-" + nuRecords.ToString().PadLeft(4, '0');
                    }
                    break;

                case Commons.ETableZipCode.DataEntry:
                    using (var db = new NuWebContext())
                    {
                        var nuRecords = db.I_DataEntry.Count(ww => DbFunctions.TruncateTime(ww.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) && ww.StoreId == storeId);
                        nuRecords++;
                        code = "DE-" + DateTime.Now.ToString("ddMMyyyy") + "-" + nuRecords.ToString().PadLeft(4, '0');
                    }
                    break;

                case Commons.ETableZipCode.StockCount:
                    using (var db = new NuWebContext())
                    {
                        var nuRecords = db.I_StockCount.Count(ww => DbFunctions.TruncateTime(ww.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) && ww.StoreId == storeId);
                        nuRecords++;
                        code = "SC-" + DateTime.Now.ToString("ddMMyyyy") + "-" + nuRecords.ToString().PadLeft(4, '0');
                    }
                    break;

                case Commons.ETableZipCode.ReceiptNoteSelfMade:
                    using (var db = new NuWebContext())
                    {
                        var nuRecords = db.I_ReceiptNoteForSeftMade.Count(ww => DbFunctions.TruncateTime(ww.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) && ww.StoreId == storeId);
                        nuRecords++;
                        code = "RNSM-" + DateTime.Now.ToString("ddMMyyyy") + "-" + nuRecords.ToString().PadLeft(4, '0');
                    }
                    break;
            }
            return code;
        }

        public static DateTime ConvertToLocalTime(DateTime dateInput)
        {
            try
            {
                var date = DateTime.SpecifyKind(dateInput, DateTimeKind.Local);
                return date;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex);
            }
            return DateTime.Now;
        }

        public static DateTime ConvertToUTCTime(DateTime dateInput)
        {
            try
            {
                var date = DateTime.SpecifyKind(dateInput, DateTimeKind.Utc);
                return date;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex);
            }
            return DateTime.Now;
        }

        public static string BusinessDayDisplay(DateTime? dFrom, DateTime? dTo)
        {
            string result = string.Empty;
            if (!dFrom.HasValue || !dTo.HasValue)
            {
                return result;
            }
            if (dFrom.Value != Commons.MinDate && dTo.Value != Commons.MinDate)
            {
                result = dFrom.Value.ToString("MM/dd/yyyy HH:mm") + " - " + dTo.Value.ToString("MM/dd/yyyy HH:mm");
            }
            else if (dTo.Value == Commons.MinDate)
            {
                result = dFrom.Value.ToString("MM/dd/yyyy HH:mm") + " - ";
            }
            return result;
        }

        public static List<CountryApiModels> GetListCountry()
        {
            List<CountryApiModels> listData = new List<CountryApiModels>();
            try
            {
                //check session
                if (System.Web.HttpContext.Current.Session["ContriesLibSessionNuWeb"] != null)
                {
                    listData = (List<CountryApiModels>)System.Web.HttpContext.Current.Session["ContriesLibSessionNuWeb"];
                }
                else
                {
                    dynamic lstResult = ApiResponse.GetWithoutHostConfig<dynamic>(ConfigurationManager.AppSettings["RestCountriesApi"].ToString(), null, null);
                    var lstContent = JsonConvert.SerializeObject(lstResult);
                    listData = JsonConvert.DeserializeObject<List<CountryApiModels>>(lstContent);
                    //write to session
                    System.Web.HttpContext.Current.Session.Add("ContriesLibSessionNuWeb", listData);
                }
                NSLog.Logger.Info("Country_GetList", listData);

                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Country_GetList: " + e);
                return listData;
            }
        }

        public static bool IsPropertyExist(dynamic data, string name)
        {
            try
            {
                if (data[name] != null)
                    return true;
                return false;
            }
            catch 
            {

                return false;
            }
        }

        public static double RoundUp2Decimal(double input)
        {
            return Math.Round(input + 0.0005, 2);
        }

        public static DateTime GetFirstDayOfWeek()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            DayOfWeek firstDay = DayOfWeek.Monday; //cultureInfo.DateTimeFormat.FirstDayOfWeek;
            DateTime firstDayInWeek = DateTime.Now.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);

            return firstDayInWeek;
        }

        public static DateTime GetLastDayOfMonth()
        {
            DateTime firstDayMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var tmp = firstDayMonth.AddMonths(1);

            return tmp.AddDays(-1);
        }

        // Get 3 months ago date and time from today's date and current time
        public static DateTime GetFirstDayOfTwoMonthsAgo()
        {
            //DateTime ago = DateTime.Now.AddMonths(-2);
            DateTime ago = DateTime.Now;
            var tmp = new DateTime(ago.Year, ago.Month, 1);

            return tmp;
        }
    }
}
