using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Sandbox.Import.Promotion;
using NuWebNCloud.Shared.Models.Sandbox.Inventory.Promotion;
using NuWebNCloud.Shared.Utilities;
//using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace NuWebNCloud.Shared.Factory.Sandbox.Inventory
{
    public class PromotionFactory : BaseFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public PromotionFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<PromotionModels> GetListPromotion(string StoreId = null, List<string> ListOrganizationId = null)
        {
            List<PromotionModels> listData = new List<PromotionModels>();
            try
            {
                PromotionApiModels paraBody = new PromotionApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.Mode = 1;
                paraBody.StoreID = StoreId;
                paraBody.ListOrgID = ListOrganizationId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetListPromotion, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["ListPromotion"];
                var lstContent = JsonConvert.SerializeObject(lstC/*result.RawData*/);
                listData = JsonConvert.DeserializeObject<List<PromotionModels>>(lstContent);
                var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                    listData = listData.Where(ww => currentUser.ListStoreID.Contains(ww.StoreID)).ToList();
                return listData;
            }
            catch (Exception e)
            {
                _logger.Error("Promotion_GetList: " + e);
                return listData;
            }
        }

        public PromotionModels GetPromotion(string StoreId = null, string PromotionId = null)
        {
            PromotionModels Promotion = new PromotionModels();
            try
            {
                PromotionApiModels paraBody = new PromotionApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.Mode = 1;
                paraBody.StoreID = StoreId;
                paraBody.ID = PromotionId;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.GetPromotion, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["PromotionData"];
                var Content = JsonConvert.SerializeObject(lstC);
                Promotion = JsonConvert.DeserializeObject<PromotionModels>(Content);
                return Promotion;
            }
            catch (Exception e)
            {
                _logger.Error("Promotion_GetDetail: " + e);
                return Promotion;
            }
        }

        public bool InsertOrUpdatePromotion(PromotionModels model, ref string msg)
        {
            try
            {
                PromotionApiModels paraBody = new PromotionApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();

                paraBody.ID = model.ID;
                paraBody.StoreID = model.StoreID;
                paraBody.Mode = 1;

                paraBody.Description = model.Description;
                paraBody.ImageURL = model.ImageURL;
                paraBody.Name = model.Name;
                paraBody.ShortName = model.ShortName;
                paraBody.PromoteCode = model.PromoteCode;
                paraBody.PromotionType = model.PromotionType ?? 0;
                paraBody.Priority = model.Priority ?? 0;
                paraBody.isActive = model.isActive ?? false;
                paraBody.IsAllowedCombined = model.IsAllowedCombined ?? false;
                paraBody.MaximumUsedQty = model.MaximumUsedQty.Value;
                paraBody.MaximumEarnAmount = model.MaximumEarnAmount;
                paraBody.IsRepeated = model.IsRepeated.Value;
                paraBody.IsLimited = model.IsLimited ?? false;
                paraBody.FromDate = new DateTime(model.FromDate.Value.Year, model.FromDate.Value.Month, model.FromDate.Value.Day, 23, 59, 59);
                paraBody.ToDate = new DateTime(model.ToDate.Value.Year, model.ToDate.Value.Month, model.ToDate.Value.Day, 23, 59, 59);
                paraBody.FromTime = model.FromTime /*?? DateTime.Now*/;
                paraBody.ToTime = model.ToTime /*?? DateTime.Now*/;
                paraBody.DateOfWeek = model.DateOfWeek;
                paraBody.DateOfMonth = model.DateOfMonth;
                paraBody.isSpendOperatorAnd = model.isSpendOperatorAnd;
                paraBody.isEarnOperatorAnd = model.isEarnOperatorAnd;
                paraBody.ListSpendingRule = model.ListSpendingRule;
                paraBody.ListEarningRule = model.ListEarningRule;
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.CreateOrEditPromotion, null, paraBody);
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
                _logger.Error("Promotion_InsertOrUpdate: " + e);
                return false;
            }
        }

        public bool DeletePromotion(string ID, ref string msg)
        {
            try
            {
                PromotionApiModels paraBody = new PromotionApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.RegisterToken = new RegisterTokenModels();
                paraBody.ID = ID;

                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.DeletePromotion, null, paraBody);
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
                _logger.Error("Promotion_Delete: " + e);
                return false;
            }
        }

        public List<PromotionImportResultItem> Import(string filePath, FileInfo[] listFileInfo, out int totalRowExel, List<string> storeIndexes, ref string msg)
        {
            totalRowExel = 0;
            List<PromotionImportResultItem> importItems = new List<PromotionImportResultItem>();

            DataTable dtPromotion = ReadExcelFile(filePath, "Promotions");
            DataTable dtSpend = ReadExcelFile(filePath, "Spending");
            DataTable dtSpendPro = ReadExcelFile(filePath, "SpendingProduct");
            DataTable dtEarn = ReadExcelFile(filePath, "Earning");
            DataTable dtEarnPro = ReadExcelFile(filePath, "EarningProduct");

            string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SBInventoryPromotion.xlsx";
            DataTable dtPromotionTmp = ReadExcelFile(tmpExcelPath, "Promotions");
            DataTable dtSpendTmp = ReadExcelFile(tmpExcelPath, "Spending");
            DataTable dtSpendProTmp = ReadExcelFile(tmpExcelPath, "SpendingProduct");
            DataTable dtEarnTmp = ReadExcelFile(tmpExcelPath, "Earning");
            DataTable dtEarnProTmp = ReadExcelFile(tmpExcelPath, "EarningProduct");

            if (dtPromotion.Columns.Count != dtPromotionTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }
            if (dtSpend.Columns.Count != dtSpendTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }
            if (dtSpendPro.Columns.Count != dtSpendProTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }
            if (dtEarn.Columns.Count != dtEarnTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }
            if (dtEarnPro.Columns.Count != dtEarnProTmp.Columns.Count)
            {
                msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                return importItems;
            }

            //List<DishImportItem> promotions = GetListObject<DishImportItem>(dtPromotion);
            //List<DishGroupImportItem> spending = GetListObject<DishGroupImportItem>(dtSpend);
            //List<DishModifierImportItem> spendingProduct = GetListObject<DishModifierImportItem>(dtSpendPro);
            //// Validate 
            //ValidateRowPromotion(ref promotions);
            //ValidateRowGroupDish(ref spending);
            //ValidateRowModifier(ref spendingProduct);

            PromotionImportResultItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            List<PromotionModels> listData = new List<PromotionModels>();
            foreach (var store in storeIndexes)
            {
                foreach (DataRow item in dtPromotion.Rows)
                {
                    flagInsert = true;
                    msgError = "";

                    if (item[0].ToString().Equals(""))
                        continue;
                    int index = int.Parse(item[0].ToString());
                    string ImageUrl = "";
                    if (!string.IsNullOrEmpty(item[14].ToString()))
                    {
                        FileInfo file = listFileInfo.FirstOrDefault(m => m.Name.ToLower() == item[14].ToString().ToLower());
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
                    //==========
                    List<SpendingRuleDTO> ListSpendingRule = new List<SpendingRuleDTO>();
                    DataRow[] Spendings = dtSpend.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Index") + "] = " + index + "");
                    foreach (DataRow itemSpend in Spendings)
                    {
                        int spendIndex = int.Parse(itemSpend[0].ToString());
                        List<PromotionProductDTO> ListPro = new List<PromotionProductDTO>();
                        DataRow[] products = dtSpendPro.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Spend Index") + "] = " + spendIndex + "");
                        foreach (DataRow product in products)
                        {
                            PromotionProductDTO productDTO = new PromotionProductDTO()
                            {
                                Name = product[2].ToString(),
                                ProductCode = product[3].ToString()
                            };

                            if (string.IsNullOrEmpty(product[1].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Spend Index is required");
                            }
                            if (string.IsNullOrEmpty(productDTO.Name))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Name is required");
                            }
                            if (string.IsNullOrEmpty(productDTO.ProductCode))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Code is required");
                            }
                            if (flagInsert)
                            {
                                ListPro.Add(productDTO);
                            }
                        }
                        SpendingRuleDTO spendDTO = new SpendingRuleDTO()
                        {
                            Condition = itemSpend[2].ToString(),

                            SpendType = string.IsNullOrEmpty(itemSpend[3].ToString()) ? (byte)Commons.ESpendType.BuyItem
                                                : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemSpend[3].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendTypeBuyItem).Replace(" ", ""))
                                                ? (byte)Commons.ESpendType.BuyItem : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemSpend[3].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendTypeSpendMoney).Replace(" ", ""))
                                                ? (byte)Commons.ESpendType.SpendMoney : (byte)0,

                            Amount = string.IsNullOrEmpty(itemSpend[4].ToString()) ? 0 : double.Parse(itemSpend[4].ToString()),

                            SpendOnType = string.IsNullOrEmpty(itemSpend[5].ToString()) ? (byte)Commons.ESpendOnType.AnyItem
                                                        : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemSpend[5].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendOnTypeAnyItem).Replace(" ", ""))
                                                        ? (byte)Commons.ESpendOnType.AnyItem : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemSpend[5].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.SpendOnTypeSpecificItem).Replace(" ", ""))
                                                        ? (byte)Commons.ESpendOnType.SpecificItem : (byte)0,

                            ListProduct = ListPro,
                        };
                        if (string.IsNullOrEmpty(itemSpend[1].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Index is required");
                        }
                        if (string.IsNullOrEmpty(itemSpend[4].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty/Amount is required");
                        }
                        if (flagInsert)
                        {
                            ListSpendingRule.Add(spendDTO);
                        }
                    }
                    //==========
                    List<EarningRuleDTO> ListEarningRule = new List<EarningRuleDTO>();
                    DataRow[] Earnings = dtEarn.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Index") + "] = " + index + "");
                    foreach (DataRow itemEarn in Earnings)
                    {
                        int earnIndex = int.Parse(itemEarn[0].ToString());
                        List<PromotionProductDTO> ListPro = new List<PromotionProductDTO>();
                        DataRow[] products = dtEarnPro.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Earn Index") + "] = " + earnIndex + "");
                        foreach (DataRow product in products)
                        {
                            PromotionProductDTO productDTO = new PromotionProductDTO()
                            {
                                Name = product[2].ToString(),
                                ProductCode = product[3].ToString()
                            };
                            if (string.IsNullOrEmpty(product[1].ToString()))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Earn Index is required");
                            }
                            if (string.IsNullOrEmpty(productDTO.Name))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Name is required");
                            }
                            if (string.IsNullOrEmpty(productDTO.ProductCode))
                            {
                                flagInsert = false;
                                msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Code is required");
                            }
                            if (flagInsert)
                            {
                                ListPro.Add(productDTO);
                            }
                        }
                        EarningRuleDTO spendDTO = new EarningRuleDTO()
                        {
                            Condition = itemEarn[2].ToString(),

                            DiscountType = string.IsNullOrEmpty(itemEarn[3].ToString()) ? true
                                                : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemEarn[3].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DiscountPercent).Replace(" ", ""))
                                                ? false : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemEarn[3].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.DiscountValue).Replace(" ", "")) ? true : false,

                            DiscountValue = string.IsNullOrEmpty(itemEarn[4].ToString()) ? 0 : double.Parse(itemEarn[4].ToString()),

                            EarnType = string.IsNullOrEmpty(itemEarn[5].ToString()) ? (byte)Commons.EEarnType.SpentItem
                            : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemEarn[5].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EarnTypeSpentItem).Replace(" ", ""))
                            ? (byte)Commons.EEarnType.SpentItem : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemEarn[5].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.EarnTypeSpecificItem).Replace(" ", ""))
                            ? (byte)Commons.EEarnType.SpecificItem : (byte)0,

                            Quantity = string.IsNullOrEmpty(itemEarn[6].ToString()) ? 0 : int.Parse(itemEarn[6].ToString()),

                            ListProduct = ListPro,
                        };
                        if (string.IsNullOrEmpty(itemEarn[1].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Index is required");
                        }
                        if (string.IsNullOrEmpty(itemEarn[4].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent/Value is required");
                        }
                        if (string.IsNullOrEmpty(itemEarn[6].ToString()))
                        {
                            flagInsert = false;
                            msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity is required");
                        }
                        if (flagInsert)
                        {
                            ListEarningRule.Add(spendDTO);
                        }

                    }
                    //======

                    bool isSpendOperatorAnd = (ListSpendingRule == null || ListSpendingRule.Count == 0) ? false : ListSpendingRule.Count == 1 ? false :
                        ListSpendingRule[1].Condition.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("AND")) ? true : false;

                    bool isEarnOperatorAnd = (ListEarningRule == null || ListEarningRule.Count == 0) ? false : ListEarningRule.Count == 1 ? false :
                        ListEarningRule[1].Condition.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("AND")) ? true : false;

                    string msgItem = "";
                    DateTime FromDate = DateTimeHelper.GetDateImport(item[12].ToString(), ref msgItem);
                    DateTime ToDate = DateTimeHelper.GetDateImport(item[13].ToString(), ref msgItem);
                    if (!msgItem.Equals(""))
                    {
                        flagInsert = false;
                        msgError = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(msgItem);
                    }

                    DateTime FromTime = Commons._MinDate;
                    DateTime ToTime = Commons._MinDate;

                    string sFromTime = string.IsNullOrEmpty(item[8].ToString()) ? "Unlimited" : item[8].ToString();
                    string sToTime = string.IsNullOrEmpty(item[9].ToString()) ? "Unlimited" : item[9].ToString();

                    if (!_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(sFromTime).Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited")))
                    {
                        msgItem = "";
                        FromTime = DateTimeHelper.GetTimeImport(sFromTime, ref msgItem);
                        if (!msgItem.Equals(""))
                        {
                            flagInsert = false;
                            msgError += msgItem;
                        }
                    }

                    if (!_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(sToTime).Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited")))
                    {
                        msgItem = "";
                        ToTime = DateTimeHelper.GetTimeImport(sToTime, ref msgItem);
                        if (!msgItem.Equals(""))
                        {
                            flagInsert = false;
                            msgError += msgItem;
                        }
                    }

                    string MaximumEarnAmount = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item[6].ToString());
                    if (string.IsNullOrEmpty(MaximumEarnAmount))
                    {
                        MaximumEarnAmount = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited");
                    }

                    PromotionModels model = new PromotionModels
                    {
                        Index = item[0].ToString(),
                        Priority = string.IsNullOrEmpty(item[1].ToString()) ? 0 : int.Parse(item[1].ToString()),
                        Name = item[2].ToString(),
                        ShortName = item[3].ToString(),
                        PromoteCode = item[4].ToString(),
                        isActive = string.IsNullOrEmpty(item[5].ToString()) ? false
                                : bool.Parse(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item[5].ToString()).Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Yes")).ToString()) ? true : false,

                        MaximumEarnAmount = MaximumEarnAmount.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited")) ? (double?)null : double.Parse(MaximumEarnAmount),
                        Description = item[7].ToString(),

                        FromTime = sFromTime.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited")) ? (DateTime?)null : FromTime,
                        ToTime = sToTime.Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Unlimited")) ? (DateTime?)null : ToTime,


                        DateOfWeek = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item[10].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PromotionDayOfWeek).Replace(" ", "").ToString()) ? item[11].ToString() : "",

                        DateOfMonth = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item[10].ToString()).Replace(" ", "").Equals(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PromotionDayOfMonth).Replace(" ", "").ToString()) ? item[11].ToString() : "",

                        FromDate = FromDate,
                        ToDate = ToDate,

                        ImageURL = ImageUrl,
                        StoreID = store,
                        ListSpendingRule = ListSpendingRule,
                        isSpendOperatorAnd = isSpendOperatorAnd,
                        ListEarningRule = ListEarningRule,
                        isEarnOperatorAnd = isEarnOperatorAnd,
                        Mode = 1
                    };
                    //Validation
                    if (string.IsNullOrEmpty(item[1].ToString()))
                    {
                        flagInsert = false;
                        msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Priority is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (string.IsNullOrEmpty(model.Name))
                    {
                        flagInsert = false;
                        msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion name is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (string.IsNullOrEmpty(model.PromoteCode))
                    {
                        flagInsert = false;
                        msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion code is required");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.FromDate.Value > model.ToDate.Value)
                    {
                        flagInsert = false;
                        msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("From Date must be less than To Date.");
                        msgError += "<br/>" + msgItem;
                    }
                    if (model.FromTime != null && model.ToTime != null)
                    {
                        if (model.FromTime.Value.TimeOfDay > model.ToTime.Value.TimeOfDay)
                        {
                            flagInsert = false;
                            msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("From time must be less than To time");
                            msgError += "<br/>" + msgItem;
                        }
                    }

                    if (model.ListSpendingRule != null)
                    {
                        foreach (var itemSpending in model.ListSpendingRule)
                        {
                            if (itemSpending.SpendOnType == (byte)Commons.ESpendOnType.SpecificItem)
                            {
                                if (itemSpending.ListProduct.Count == 0)
                                {
                                    flagInsert = false;
                                    msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please select specific items in item detail for spending rule no.") + index;
                                    msgError += "<br/>" + msgItem;
                                    break;
                                }
                            }
                            if (itemSpending.Amount < 0)
                            {
                                flagInsert = false;
                                msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter value of Quantity/Amount for spending rule no.") + index;
                                msgError += "<br/>" + msgItem;
                                break;
                            }
                            else
                            {
                                itemSpending.Amount = Math.Round(itemSpending.Amount, 2);
                            }
                            index++;
                        }
                    }
                    if (model.ListEarningRule != null)
                    {
                        foreach (var itemEarning in model.ListEarningRule)
                        {
                            if (itemEarning.EarnType == (byte)Commons.EEarnType.SpecificItem)
                            {
                                if (itemEarning.ListProduct.Count == 0)
                                {
                                    flagInsert = false;
                                    msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please select specific items in item detail for earning rule no.") + index;
                                    msgError += "<br/>" + msgItem;
                                    break;
                                }
                            }
                            //=======
                            if (itemEarning.bDiscountType == (byte)Commons.EValueType.Percent)
                            {
                                if (itemEarning.DiscountValue < 0 || itemEarning.DiscountValue > 100)
                                {
                                    flagInsert = false;
                                    msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Discount Percent could not larger than 100%");
                                    msgError += "<br/>" + msgItem;
                                    break;
                                }
                            }
                            else
                            {
                                if (itemEarning.DiscountValue < 0)
                                {
                                    flagInsert = false;
                                    msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter value of Percent/Value for earning rule no.") + index;
                                    msgError += "<br/>" + msgItem;
                                    break;
                                }
                                else
                                {
                                    itemEarning.DiscountValue = Math.Round(itemEarning.DiscountValue, 2);
                                }
                            }
                            if (itemEarning.Quantity < 0)
                            {
                                flagInsert = false;
                                msgItem = "" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Please enter value of Quantity for earning rule no.") + index;
                                msgError += "<br/>" + msgItem;
                                break;
                            }
                            index++;
                        }
                    }
                    if (flagInsert)
                    {
                        listData.Add(model);
                    }
                    else
                    {
                        PromotionErrorItem itemerr = new PromotionErrorItem();
                        itemerr.GroupName = model.Index;
                        itemerr.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + model.Index + msgError;

                        itemErr = new PromotionImportResultItem();
                        itemErr.Name = model.Name;
                        itemErr.ListFailStoreName.Add("");
                        itemErr.ErrorItems.Add(itemerr);
                        importItems.Add(itemErr);
                    }
                }
            }

            //try
            //{
            PromotionApiModels paraBody = new PromotionApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.RegisterToken = new RegisterTokenModels();
            paraBody.ListPromotion = listData;

            //====================
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ImportPromotion, null, paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                foreach (ImportResult itemError in listError)
                {
                    PromotionErrorItem item = new PromotionErrorItem();
                    item.GroupName = itemError.Index;
                    item.ErrorMessage = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + itemError.Index + "<br/>"
                        + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error);

                    PromotionImportResultItem importItem = new PromotionImportResultItem();
                    importItem.Name = itemError.Property;
                    importItem.ListFailStoreName.Add(itemError.StoreName);
                    importItem.ErrorItems.Add(item);
                    importItems.Add(importItem);
                }
                if (importItems.Count == 0)
                {
                    PromotionImportResultItem importItem = new PromotionImportResultItem();
                    importItem.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion");
                    importItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Promotion Successful"));
                    importItems.Add(importItem);
                }
            }
            return importItems;
        }

        public StatusResponse Export(ref IXLWorksheet wsPromotions, ref IXLWorksheet wsSpending, ref IXLWorksheet wsSpendingProduct,
            ref IXLWorksheet wsEarning, ref IXLWorksheet wsEarningProduct, List<string> lstStore)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                List<PromotionModels> listData = new List<PromotionModels>();
                PromotionApiModels paraBody = new PromotionApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.Mode = 1;
                paraBody.ListStoreID = lstStore;

                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ExportPromotion, null, paraBody);
                dynamic data = result.Data;
                var lstC = data["PromotionData"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                listData = JsonConvert.DeserializeObject<List<PromotionModels>>(lstContent);
                listData = listData.OrderBy(o => o.StoreName).ToList();

                int row = 1;
                string[] listPromotionHeader = new string[] {
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Priority"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Short Name"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Code"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Maximun Amount"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Description"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("From Time"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To Time"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Date Type"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Days"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("From Date"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To Date"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Image Url"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Store")
                };
                for (int i = 1; i <= listPromotionHeader.Length; i++)
                    wsPromotions.Cell(row, i).Value = listPromotionHeader[i - 1];
                int cols = listPromotionHeader.Length;
                //Item
                row = 2;
                int countIndex = 1;
                int countSpend = 1;
                int countSpendPro = 1;
                int countEarn = 1;
                int countEarnPro = 1;
                List<ExportSpending> lstSpend = new List<ExportSpending>();
                List<ExportSpendingProduct> lstSpendPro = new List<ExportSpendingProduct>();

                List<ExportEarning> lstEarn = new List<ExportEarning>();
                List<ExportEarningProduct> lstEarnPro = new List<ExportEarningProduct>();

                foreach (var item in listData)
                {
                    if (item.FromTime == null || item.ToTime == null)
                        item.IsLimitedTime = true;
                    else
                        item.IsLimitedTime = false;
                    if (!string.IsNullOrEmpty(item.DateOfWeek))
                        item.RepeatType = 2;
                    else if (!string.IsNullOrEmpty(item.DateOfMonth))
                        item.RepeatType = 3;
                    wsPromotions.Cell("A" + row).Value = countIndex;
                    wsPromotions.Cell("B" + row).Value = item.Priority;
                    wsPromotions.Cell("C" + row).Value = item.Name;
                    wsPromotions.Cell("D" + row).Value = item.ShortName;
                    wsPromotions.Cell("E" + row).Value = item.PromoteCode;
                    wsPromotions.Cell("F" + row).Value = item.isActive.Value ? "Yes" : "No";
                    wsPromotions.Cell("G" + row).Value = (item.MaximumEarnAmount.HasValue ? item.MaximumEarnAmount.Value.ToString() : "Unlimited");
                    wsPromotions.Cell("H" + row).Value = item.Description;
                    wsPromotions.Cell("I" + row).Value = item.IsLimitedTime ? "Unlimited" : "'" + item.FromTime.Value.ToLocalTime().ToString("HH:mm");
                    wsPromotions.Cell("J" + row).Value = item.IsLimitedTime ? "Unlimited" : "'" + item.ToTime.Value.ToLocalTime().ToString("HH:mm");

                    wsPromotions.Cell("K" + row).Value = item.RepeatType == (byte)Commons.ERepeatType.DayOfWeek ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PromotionDayOfWeek) :
                                                 item.RepeatType == (byte)Commons.ERepeatType.DayOfMonth ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons.PromotionDayOfMonth) :
                                                 _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Other");

                    wsPromotions.Cell("L" + row).Value = !string.IsNullOrEmpty(item.DateOfWeek) ? "'" + item.DateOfWeek : "'" + item.DateOfMonth;
                    wsPromotions.Cell("M" + row).Value = "'" + item.FromDate.Value.ToString("dd/MM/yyyy");
                    wsPromotions.Cell("N" + row).Value = "'" + item.ToDate.Value.ToString("dd/MM/yyyy");
                    wsPromotions.Cell("O" + row).Value = item.ImageURL.Replace(Commons._PublicImages, "");
                    wsPromotions.Cell("P" + row).Value = item.StoreName;
                    //=======SpendingRule
                    if (item.ListSpendingRule != null)
                    {
                        for (int i = 0; i < item.ListSpendingRule.Count; i++)
                        {
                            var itemSpend = item.ListSpendingRule[i];
                            if (i > 0)
                                itemSpend.Condition = item.isSpendOperatorAnd ? "AND" : "OR";
                            ExportSpending spend = new ExportSpending()
                            {
                                SpendIndex = countSpend,
                                PromotionIndex = countIndex,
                                Condition = itemSpend.Condition,
                                QtyAmount = itemSpend.Amount,

                                SpendType = itemSpend.SpendType == (byte)Commons.ESpendType.BuyItem ? Commons.SpendTypeBuyItem :
                                                 itemSpend.SpendType == (byte)Commons.ESpendType.SpendMoney ? Commons.SpendTypeSpendMoney : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Other"),

                                Item = itemSpend.SpendOnType == (byte)Commons.ESpendOnType.AnyItem ? Commons.SpendOnTypeAnyItem :
                                                            itemSpend.SpendOnType == (byte)Commons.ESpendOnType.SpecificItem ? Commons.SpendOnTypeSpecificItem : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Other"),
                            };
                            if (itemSpend.ListProduct != null)
                            {
                                for (int j = 0; j < itemSpend.ListProduct.Count; j++)
                                {
                                    ExportSpendingProduct spendPro = new ExportSpendingProduct()
                                    {
                                        SpendProIndex = countSpendPro,
                                        SpendIndex = countSpend,
                                        Name = itemSpend.ListProduct[j].Name,
                                        Code = itemSpend.ListProduct[j].ProductCode,
                                    };
                                    countSpendPro++;
                                    lstSpendPro.Add(spendPro);
                                }
                            }
                            countSpend++;
                            lstSpend.Add(spend);
                        }
                    }
                    //=======EarningRule
                    if (item.ListEarningRule != null)
                    {
                        for (int i = 0; i < item.ListEarningRule.Count; i++)
                        {
                            var itemEarn = item.ListEarningRule[i];
                            //=======
                            if (i > 0)
                                itemEarn.Condition = item.isEarnOperatorAnd ? "AND" : "OR";

                            ExportEarning earn = new ExportEarning()
                            {
                                EarnIndex = countEarn,
                                PromotionIndex = countIndex,
                                Condition = itemEarn.Condition,
                                PercentValue = itemEarn.DiscountValue,
                                Qty = itemEarn.Quantity,

                                EarnType = !itemEarn.DiscountType ? Commons.DiscountPercent : itemEarn.DiscountType ? Commons.DiscountValue : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Other"),
                                Item = itemEarn.EarnType == (byte)Commons.EEarnType.SpentItem ? Commons.EarnTypeSpentItem :
                                                            itemEarn.EarnType == (byte)Commons.EEarnType.SpecificItem ? Commons.EarnTypeSpecificItem : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Other"),
                            };
                            if (itemEarn.ListProduct != null)
                            {
                                for (int j = 0; j < itemEarn.ListProduct.Count; j++)
                                {
                                    ExportEarningProduct earnPro = new ExportEarningProduct()
                                    {
                                        EarnProIndex = countEarnPro,
                                        EarnIndex = countEarn,
                                        Name = itemEarn.ListProduct[j].Name,
                                        Code = itemEarn.ListProduct[j].ProductCode,
                                    };
                                    countEarnPro++;
                                    lstEarnPro.Add(earnPro);
                                }
                            }
                            countEarn++;
                            lstEarn.Add(earn);
                        }
                    }

                    row++;
                    countIndex++;
                }
                FormatExcelExport(wsPromotions, row, cols);
                //========= Spending
                row = 1;
                string[] listSpendingHeader = new string[] {
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Spend Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Condition") ,
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Spend Type"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty/Amount"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item")
                };
                for (int i = 1; i <= listSpendingHeader.Length; i++)
                    wsSpending.Cell(row, i).Value = listSpendingHeader[i - 1];
                cols = listSpendingHeader.Length;
                row++;
                foreach (var item in lstSpend)
                {
                    wsSpending.Cell("A" + row).Value = item.SpendIndex;
                    wsSpending.Cell("B" + row).Value = item.PromotionIndex;
                    wsSpending.Cell("C" + row).Value = item.Condition;
                    wsSpending.Cell("D" + row).Value = item.SpendType;
                    wsSpending.Cell("E" + row).Value = item.QtyAmount;
                    wsSpending.Cell("F" + row).Value = item.Item;
                    row++;
                }
                FormatExcelExport(wsSpending, row, cols);
                //============ SpendingProduct
                row = 1;
                string[] listSpendingProductHeader = new string[] {
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Spend Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Name"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Code")
                };
                for (int i = 1; i <= listSpendingProductHeader.Length; i++)
                    wsSpendingProduct.Cell(row, i).Value = listSpendingProductHeader[i - 1];
                cols = listSpendingProductHeader.Length;
                row++;
                foreach (var item in lstSpendPro)
                {
                    wsSpendingProduct.Cell("A" + row).Value = item.SpendProIndex;
                    wsSpendingProduct.Cell("B" + row).Value = item.SpendIndex;
                    wsSpendingProduct.Cell("C" + row).Value = item.Name;
                    wsSpendingProduct.Cell("D" + row).Value = "'" + item.Code;
                    row++;
                }
                FormatExcelExport(wsSpendingProduct, row, cols);
                //========Earning
                row = 1;
                string[] listEarningHeader = new string[] {
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Earn Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Promotion Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Condition") ,
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Earn Type"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Percent/Value"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Qty")
                };
                for (int i = 1; i <= listEarningHeader.Length; i++)
                    wsEarning.Cell(row, i).Value = listEarningHeader[i - 1];
                cols = listEarningHeader.Length;
                row++;
                foreach (var item in lstEarn)
                {
                    wsEarning.Cell("A" + row).Value = item.EarnIndex;
                    wsEarning.Cell("B" + row).Value = item.PromotionIndex;
                    wsEarning.Cell("C" + row).Value = item.Condition;
                    wsEarning.Cell("D" + row).Value = item.EarnType;
                    wsEarning.Cell("E" + row).Value = item.PercentValue;
                    wsEarning.Cell("F" + row).Value = item.Item;
                    wsEarning.Cell("G" + row).Value = item.Qty;
                    row++;
                }
                FormatExcelExport(wsEarning, row, cols);
                //============EarningProduct
                row = 1;
                string[] listEarningProductHeader = new string[] {
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Earn Index"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Name"),
                     _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Product Code")
                };
                for (int i = 1; i <= listEarningProductHeader.Length; i++)
                    wsEarningProduct.Cell(row, i).Value = listEarningProductHeader[i - 1];
                cols = listEarningProductHeader.Length;
                row++;
                foreach (var item in lstEarnPro)
                {
                    wsEarningProduct.Cell("A" + row).Value = item.EarnProIndex;
                    wsEarningProduct.Cell("B" + row).Value = item.EarnIndex;
                    wsEarningProduct.Cell("C" + row).Value = item.Name;
                    wsEarningProduct.Cell("D" + row).Value = "'" + item.Code;
                    row++;
                }
                FormatExcelExport(wsEarningProduct, row, cols);
                //========
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
