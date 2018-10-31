using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Integration.Models.Sandbox;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Integration.Factory.Sandbox
{
    public class InteEmployeeFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public InteEmployeeFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<InteEmployeeModels> GetListEmployee(string StoreID = null, string EmployeeId = null, List<string> ListOrganizationId = null)
        {
            List<InteEmployeeModels> listData = new List<InteEmployeeModels>();
            try
            {
                InteSBEmployeeApiModels paraBody = new InteSBEmployeeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.StoreID = StoreID;
                paraBody.ID = EmployeeId;
                //paraBody.Mode = 1;
                paraBody.ListOrgID = ListOrganizationId;
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetEmployee, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListEmployee"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<InteEmployeeModels>>(lstContent);
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Employee_GetList: " + e);
                return listData;
            }
        }

        public bool InsertOrUpdateEmployee(InteEmployeeModels model, ref string msg)
        {
            try
            {
                InteSBEmployeeApiModels paraBody = new InteSBEmployeeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                paraBody.EmployeeDTO = model;

                //====================
                NSLog.Logger.Info("InsertOrUpdateEmployee Info", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditEmployee, null, paraBody);
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
                _logger.Error("Employee_InsertOrUpdate: " + e);
                //msg = e.ToString();
                return false;
            }
        }

        public bool DeleteEmployee(string ID, string StoreID, ref string msg)
        {
            try
            {
                InteSBEmployeeApiModels paraBody = new InteSBEmployeeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = ID;
                paraBody.StoreID = StoreID;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeleteEmployee, null, paraBody);
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
                _logger.Error("Employee_Delete: " + e);
                return false;
            }
        }

        public ImportModel Import(string filePath, FileInfo[] listFileInfo, List<string> storeIndexes, ref string msg)
        {

            ImportModel importItems = new ImportModel();

            DataTable dtEmployee = ReadExcelFile(filePath, "Sheet1");
            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBEmployeeInte.xlsx";
            DataTable dtEmployeeTmp = ReadExcelFile(tmpExcelPath, "Sheet1");

            if (dtEmployee.Columns.Count != dtEmployeeTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }

            ImportItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            List<InteEmployeeModels> listData = new List<InteEmployeeModels>();
            //foreach (var store in storeIndexes)
            {
                foreach (DataRow item in dtEmployee.Rows)
                {
                    flagInsert = true;
                    msgError = "";

                    if (item[0].ToString().Equals(""))
                        continue;
                    int index = int.Parse(item[0].ToString());

                    string ImageUrl = "";
                    if (!string.IsNullOrEmpty(item[15].ToString()))
                    {
                        FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[15].ToString().ToLower());
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

                    List<InteEmployeeOnStoreModels> ListEmpStore = new List<InteEmployeeOnStoreModels>();
                    foreach (var storeId in storeIndexes)
                    {
                        //===== Role Name
                        string RoleName = item[6].ToString();
                        if (string.IsNullOrEmpty(RoleName))
                        {
                            flagInsert = false;
                            msgError += "<br/>"+ _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(("Role Name is required"));
                        }
                        ListEmpStore.Add(new InteEmployeeOnStoreModels
                        {
                            StoreID = storeId,
                            RoleName = RoleName
                        });
                    }

                    string msgItem = "";
                    DateTime HiredDate = DateTimeHelper.GetDateImport(item[8].ToString(), ref msgItem);
                    DateTime BirthDate = DateTimeHelper.GetDateImport(item[9].ToString(), ref msgItem);

                    if (!msgItem.Equals(""))
                    {
                        flagInsert = false;
                        msgError = msgItem;
                    }
                    InteEmployeeModels model = new InteEmployeeModels
                    {
                        Index = item[0].ToString(),
                        Name = item[1].ToString(),
                        Pincode = item[2].ToString(),
                        Email = item[3].ToString(),
                        Phone = item[4].ToString(),
                        Gender = string.IsNullOrEmpty(item[5].ToString()) ? false : item[5].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Male")) ? true : false,
                        Marital = string.IsNullOrEmpty(item[7].ToString()) ? false : item[7].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Married")) ? true : false,
                        HiredDate = item[8].ToString().Equals("") ? Commons._ExpiredDate : HiredDate,
                        BirthDate = item[9].ToString().Equals("") ? Commons._ExpiredDate : BirthDate,
                        IsActive = string.IsNullOrEmpty(item[10].ToString()) ? false
                                        : bool.Parse(item[10].ToString().ToLower().Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active")).ToString()) ? true : false,
                        Country = item[11].ToString(),
                        Zipcode = item[12].ToString(),
                        City = item[13].ToString(),
                        Street = item[14].ToString(),
                        ImageURL = ImageUrl,
                        ListEmpStore = ListEmpStore
                    };
                    //=========
                    //===== Name
                    if (string.IsNullOrEmpty(model.Name))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name is required");
                        msgError += "<br/>" + msgItem;
                    }
                    //===== PinCode
                    if (model.Pincode.Equals(""))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Pin Code is required");
                        msgError += "<br/>" + msgItem;
                    }
                    else if (model.Pincode.Length < 3 || model.Pincode.Length > 20)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Number of characters should be between 3 and 20");
                        msgError += "<br/>" + msgItem;
                    }
                    else if (!CommonHelper.IsNumber(model.Pincode))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a valid PIN Code");
                        msgError += "<br/>" + msgItem;
                    }
                    //===== Email
                    if (model.Email.Equals(""))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Email is required");
                        msgError += "<br/>" + msgItem;
                    }
                    else if (!CommonHelper.IsValidEmail(model.Email))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a valid email address.");
                        msgError += "<br/>" + msgItem;
                    }
                    //=======Phone
                    if (model.Phone.Equals(""))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Phone number is required");
                        msgError += "<br/>" + msgItem;
                    }
                    else if (!CommonHelper.IsNumber(model.Phone))
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter a valid phone number");
                        msgError += "<br/>" + msgItem;
                    }
                    //===== Name
                    if (model.HiredDate.Date > DateTime.Now.Date)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hired date must be before or equal to the current date");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.BirthDate.Date > DateTime.Now.Date)
                    {
                        flagInsert = false;
                        msgItem = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Birth date must be before or equal to the current date");
                        msgError += "<br/>" + msgItem;
                    }
                    //===========
                    if (flagInsert)
                    {
                        listData.Add(model);
                    }
                    else
                    {
                        itemErr = new ImportItem();
                        itemErr.Name = model.Name;
                        itemErr.ListFailStoreName.Add("");
                        itemErr.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + index + msgError);
                        importItems.ListImport.Add(itemErr);
                    }
                }
            }

            InteSBEmployeeApiModels paraBody = new InteSBEmployeeApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListEmployee = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportEmployee, null, paraBody);
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
                    importItems.ListImport.Add(item);
                }
                if (importItems.ListImport.Count == 0)
                {
                    ImportItem item = new ImportItem();
                    item.Name = "Employee";
                    item.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Employee Successful"));
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
                List<InteEmployeeModels> listData = new List<InteEmployeeModels>();

                InteSBEmployeeApiModels paraBody = new InteSBEmployeeApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportEmployee, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListEmployee"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<InteEmployeeModels>>(lstContent);

                int row = 1;
                string[] listSetMenuHeader = new string[] {
                    _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Employee Name")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Pin Code")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Email")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Phone Number")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Gender")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Role Name")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Marital Status")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Hired Date")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Birthday")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Country")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Zip Code")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("City")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Street")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image Url")
                    , _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store Name")
                };
                for (int i = 1; i <= listSetMenuHeader.Length; i++)
                    ws.Cell(row, i).Value = listSetMenuHeader[i - 1];
                int cols = listSetMenuHeader.Length;
                //Item
                row = 2;
                int countIndex = 1;



                foreach (var item in listData)
                {
                    foreach (var emp in item.ListEmpStore)
                    {
                        ws.Cell("A" + row).Value = countIndex;
                        ws.Cell("B" + row).Value = item.Name;
                        ws.Cell("C" + row).Value = "'" + item.Pincode;
                        ws.Cell("D" + row).Value = item.Email;
                        ws.Cell("E" + row).Value = "'" + item.Phone;
                        ws.Cell("F" + row).Value = item.Gender ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Male") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Female");
                        ws.Cell("G" + row).Value = emp.RoleName;
                        ws.Cell("H" + row).Value = item.Marital ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Married") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Single");
                        ws.Cell("I" + row).Value = "'" + item.HiredDate.ToString("dd/MM/yyyy");
                        ws.Cell("J" + row).Value = "'" + item.BirthDate.ToString("dd/MM/yyyy");
                        ws.Cell("K" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");
                        ws.Cell("L" + row).Value = item.Country;
                        ws.Cell("M" + row).Value = "'" + item.Zipcode;
                        ws.Cell("N" + row).Value = item.City;
                        ws.Cell("O" + row).Value = item.Street;
                        ws.Cell("P" + row).Value = item.ImageURL.Replace(Commons._PublicImages, "");
                        ws.Cell("Q" + row).Value = emp.StoreName;

                        row++;
                        countIndex++;
                    }
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
