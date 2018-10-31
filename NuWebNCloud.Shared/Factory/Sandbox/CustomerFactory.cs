using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
//using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Sandbox
{
    public class CustomerFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public CustomerFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<CustomerModels> GetListCustomer(string StoreID = null, string CustomerId = null, List<string> ListOrganizationId = null)
        {
            List<CustomerModels> listData = new List<CustomerModels>();
            try
            {
                CustomerApiModels paraBody = new CustomerApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = CustomerId;
                paraBody.Mode = 1;
                paraBody.ListOrgID = ListOrganizationId;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetCustomer, null, paraBody);
                dynamic data = result.Data;
                string obj = "";
                if (!string.IsNullOrEmpty(CustomerId))
                {
                    obj = "CustomerDTO";
                    var lstC = data[obj];
                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                    CustomerModels cusDTO = JsonConvert.DeserializeObject<CustomerModels>(lstContent);
                    cusDTO.StoreID = cusDTO.ListStore[0].StoreID;
                    listData.Add(cusDTO);
                }
                else
                {
                    obj = "ListCus";
                    var lstC = data[obj];
                    var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                    listData = JsonConvert.DeserializeObject<List<CustomerModels>>(lstContent);
                }
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listData = listData.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();

                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Customer_GetList: " + e);
                return listData;
            }
        }

        public bool InsertOrUpdateCustomer(CustomerModels model, ref string msg)
        {
            try
            {
                CustomerApiModels paraBody = new CustomerApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                paraBody.CustomerDTO = model;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditCustomer, null, paraBody);
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
                    //msg = result.ToString();
                    _logger.Error(result);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Customer_InsertOrUpdate: " + e);
                //msg = e.ToString();
                return false;
            }
        }

        public bool DeleteCustomer(string ID, string StoreID, ref string msg)
        {
            try
            {
                CustomerApiModels paraBody = new CustomerApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;
                paraBody.StoreID = StoreID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteCustomer, null, paraBody);
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
                _logger.Error("Customer_Delete: " + e);
                return false;
            }
        }

        public ImportModel Import(string filePath, FileInfo[] listFileInfo, List<string> storeIndexes, ref string msg)
        {

            ImportModel importItems = new ImportModel();

            DataTable dtCustomer = ReadExcelFile(filePath, "Sheet1");
            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBCustomer.xlsx";
            DataTable dtCustomerTmp = ReadExcelFile(tmpExcelPath, "Sheet1");

            if (dtCustomer.Columns.Count != dtCustomerTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }

            ImportItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            List<CustomerModels> listData = new List<CustomerModels>();
            foreach (var store in storeIndexes)
            {
                foreach (DataRow item in dtCustomer.Rows)
                {
                    flagInsert = true;
                    msgError = "";

                    if (item[0].ToString().Equals(""))
                        continue;
                    int index = int.Parse(item[0].ToString());

                    string ImageUrl = "";
                    if (!string.IsNullOrEmpty(item[21].ToString()))
                    {
                        FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[21].ToString().ToLower());
                        if (file != null)
                        {
                            if (file.Length > Commons._MaxSizeFileUploadImg)
                            {
                                flagInsert = false;
                                msgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgAllowedSizeImg) + "<br/>";
                            }
                            else
                            {
                                ImageUrl = Guid.NewGuid() + file.Extension;
                                byte[] photoByte = null;
                                photoByte = System.IO.File.ReadAllBytes(file.FullName);
                                //19/01/2018
                                //photoByte = file.ReadFully();
                                //Save Image on Server
                                if (!string.IsNullOrEmpty(ImageUrl) && photoByte != null)
                                {
                                    var originalDirectory = new DirectoryInfo(string.Format("{0}Uploads\\", System.Web.HttpContext.Current.Server.MapPath(@"\")));
                                    var path = string.Format("{0}{1}", originalDirectory, ImageUrl);
                                    MemoryStream ms = new MemoryStream(photoByte, 0, photoByte.Length);
                                    ms.Write(photoByte, 0, photoByte.Length);
                                    System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);
                                    ImageHelper.Me.SaveCroppedImage(imageTmp, path, ImageUrl, ref photoByte);
                                    FTP.Upload(ImageUrl, photoByte);
                                    ImageHelper.Me.TryDeleteImageUpdated(path);
                                }
                            }
                        }
                    }

                    string msgItem = "";
                    DateTime JoinedDate = DateTimeHelper.GetDateImport(item[9].ToString(), ref msgItem);
                    DateTime BirthDate = DateTimeHelper.GetDateImport(item[10].ToString(), ref msgItem);
                    DateTime Anniversary = item[11].ToString().Equals("") ? Commons._ExpiredDate
                                : DateTimeHelper.GetDateImport(item[11].ToString(), ref msgItem);
                    if (!msgItem.Equals(""))
                    {
                        flagInsert = false;
                        msgError += _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msgItem);
                    }

                    CustomerModels model = new CustomerModels
                    {
                        Index = item[0].ToString(),
                        Name = item[1].ToString(),
                        IsActive = bool.Parse(item[2].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active")).ToString()) ? true : false,
                        Email = item[3].ToString(),
                        Phone = item[4].ToString(),
                        Gender = item[5].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Male")) ? true : false,
                        Marital = item[6].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Married")) ? true : false,
                        IsMembership = bool.Parse(item[7].ToString().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        JoinedDate = item[9].ToString().Equals("") ? Commons._ExpiredDate : JoinedDate,
                        BirthDate = item[10].ToString().Equals("") ? Commons._ExpiredDate : BirthDate,
                        Anniversary = item[11].ToString().Equals("") ? Commons._ExpiredDate : Anniversary,

                        IC = item[12].ToString(),

                        OfficeCountry = item[13].ToString(),
                        OfficeZipCode = item[14].ToString(),
                        OfficeCity = item[15].ToString(),
                        OfficeStreet = item[16].ToString(),

                        HomeCountry = item[17].ToString(),
                        HomeZipCode = item[18].ToString(),
                        HomeCity = item[19].ToString(),
                        HomeStreet = item[20].ToString(),

                        ImageURL = ImageUrl,
                        StoreID = store,

                        LastVisited = DateTime.Now
                    };

                    if (flagInsert)
                    {
                        listData.Add(model);
                    }
                    else
                    {
                        itemErr = new ImportItem();
                        itemErr.Name = model.Name;
                        itemErr.ListFailStoreName.Add("");
                        itemErr.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + index + ": " + msgError);
                        importItems.ListImport.Add(itemErr);
                    }
                }
            }

            CompanyApiModels paraBody = new CompanyApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListCustomer = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportCustomer, null, paraBody);
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
                    item.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index 
                        + "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error));
                    importItems.ListImport.Add(item);
                }
                if (importItems.ListImport.Count == 0)
                {
                    ImportItem item = new ImportItem();
                    item.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Customers");
                    item.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Customer Successful"));
                    importItems.ListImport.Add(item);
                }
            }
            return importItems;
        }

        public StatusResponse Export(ref IXLWorksheet ws, List<string> lstStore)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<CustomerModels> listData = new List<CustomerModels>();

                CustomerApiModels paraBody = new CustomerApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportCustomer, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListCustomer"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<CustomerModels>>(lstContent);

                int row = 1;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Customer Name"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Email"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Phone Number"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Gender"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Marital Status"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Allow Membership"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Use Wallet"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("JoinDate"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Birthday"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Anniversary"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Identification"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Office Country"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Office Zip code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Office City"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Office Street"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Home Country"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Home Zip code"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Home City"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Home Street"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image Url"),
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store/Area/Group Name")
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
                    ws.Cell("C" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active")
                                            : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");
                    ws.Cell("D" + row).Value = item.Email;
                    ws.Cell("E" + row).Value = "'" + item.Phone;
                    ws.Cell("F" + row).Value = item.Gender ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Male")
                                        : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Female");
                    ws.Cell("G" + row).Value = item.Marital ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Married")
                                        : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Single");
                    ws.Cell("H" + row).Value = item.IsMembership ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")
                                        : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("No");
                    ws.Cell("I" + row).Value = "";
                    ws.Cell("J" + row).Value = "'" + item.JoinedDate.ToString("dd/MM/yyyy");
                    ws.Cell("K" + row).Value = "'" + item.BirthDate.ToString("dd/MM/yyyy");
                    ws.Cell("L" + row).Value = item.Marital ? "'" + item.Anniversary.ToString("dd/MM/yyyy") : "";
                    ws.Cell("M" + row).Value = "'" + item.IC;

                    ws.Cell("N" + row).Value = item.OfficeCountry;
                    ws.Cell("O" + row).Value = "'" + item.OfficeZipCode;
                    ws.Cell("P" + row).Value = item.OfficeCity;
                    ws.Cell("Q" + row).Value = item.OfficeStreet;

                    ws.Cell("R" + row).Value = item.HomeCountry;
                    ws.Cell("S" + row).Value = "'" + item.HomeZipCode;
                    ws.Cell("T" + row).Value = item.HomeCity;
                    ws.Cell("U" + row).Value = item.HomeStreet;
                    ws.Cell("V" + row).Value = item.ImageURL.Replace(Commons._PublicImages, "");

                    ws.Cell("W" + row).Value = "";

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
