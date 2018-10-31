using ClosedXML.Excel;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Factory.Sandbox.Inventory.Product;
using NuWebNCloud.Shared.Factory.Xero;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Ingredients;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Xero.Suppliers;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace NuWebNCloud.Shared.Factory.Ingredients
{
    public class SupplierFactory : BaseFactory
    {
        private BaseFactory _baseFactory = null;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SupplierFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public bool Insert(SupplierModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        I_Supplier item = new I_Supplier();
                        string SupplierId = Guid.NewGuid().ToString();
                        item.Id = SupplierId;
                        item.CompanyId = model.CompanyId;
                        item.Name = model.Name;
                        item.Address = model.Address;
                        item.City = model.City;
                        item.Country = model.Country;
                        item.ZipCode = model.ZipCode;
                        item.Phone1 = model.Phone1;
                        item.Phone2 = model.Phone2;
                        item.Fax = model.Fax;
                        item.Email = model.Email;
                        item.ContactInfo = model.ContactInfo;
                        item.CreatedBy = model.CreatedBy;
                        item.CreatedDate = model.CreatedDate;
                        item.ModifierDate = model.ModifierDate;
                        item.ModifierBy = model.ModifierBy;
                        item.IsActived = model.IsActived;
                        item.Status = (int)Commons.EStatus.Actived;

                        //Supplier Ingredient
                        List<I_Ingredient_Supplier> LstSupplierIng = new List<I_Ingredient_Supplier>();
                        foreach (var SupIng in model.ListSupIng)
                        {
                            LstSupplierIng.Add(new I_Ingredient_Supplier
                            {
                                Id = Guid.NewGuid().ToString(),
                                IngredientId = SupIng.IngredientId,
                                SupplierId = SupplierId,
                                CreatedBy = model.CreatedBy,
                                CreatedDate = model.CreatedDate,
                                ModifierDate = model.ModifierDate,
                                ModifierBy = model.ModifierBy,
                                IsActived = true
                            });
                        }
                        cxt.I_Supplier.Add(item);
                        cxt.I_Ingredient_Supplier.AddRange(LstSupplierIng);
                        cxt.SaveChanges();
                        transaction.Commit();

                        /* INSERT OR UPDATE CONTACTS XERO  */
                        InsertOrUpdateContactXero("insert", model, SupplierId);
                    }
                    catch (Exception ex)
                    {
                        NSLog.Logger.Error("Insert Supplier error:", ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            return result;
        }

        public bool Update(SupplierModels model, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    try
                    {
                        var itemUpdate = (from tb in cxt.I_Supplier
                                          where tb.Id == model.Id
                                          select tb).FirstOrDefault();
                        if (itemUpdate != null)
                        {
                            itemUpdate.Name = model.Name;
                            itemUpdate.Address = model.Address;
                            itemUpdate.City = model.City;
                            itemUpdate.Country = model.Country;
                            itemUpdate.ZipCode = model.ZipCode;
                            itemUpdate.Phone1 = model.Phone1;
                            itemUpdate.Phone2 = model.Phone2;
                            itemUpdate.Fax = model.Fax;
                            itemUpdate.Email = model.Email;
                            itemUpdate.ContactInfo = model.ContactInfo;

                            itemUpdate.ModifierDate = model.ModifierDate;
                            itemUpdate.ModifierBy = model.ModifierBy;

                            itemUpdate.IsActived = model.IsActived;
                            /*Detail*/
                            var ListDelSupIng = (from IS in cxt.I_Ingredient_Supplier
                                                 where IS.SupplierId.Equals(model.Id) && IS.IsActived == true
                                                 select IS.Id).ToList();
                            if (ListDelSupIng.Count > 0)
                            {
                                //ListDelSupIng.ForEach(x => x.IsActived = false);
                                cxt.I_Ingredient_Supplier.Where(w => ListDelSupIng.Contains(w.Id)).ForEach(x => x.IsActived = false);
                            }
                            List<I_Ingredient_Supplier> LstSupplierIng = new List<I_Ingredient_Supplier>();
                            foreach (var SupIng in model.ListSupIng)
                            {
                                LstSupplierIng.Add(new I_Ingredient_Supplier
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IngredientId = SupIng.IngredientId,
                                    SupplierId = model.Id,
                                    CreatedBy = model.ModifierBy,
                                    CreatedDate = model.ModifierDate,
                                    ModifierDate = model.ModifierDate,
                                    ModifierBy = model.ModifierBy,
                                    IsActived = true
                                });
                            }
                            cxt.I_Ingredient_Supplier.AddRange(LstSupplierIng);
                            cxt.SaveChanges();
                            transaction.Commit();

                            /* INSERT OR UPDATE CONTACTS XERO  */
                            InsertOrUpdateContactXero("update", model, model.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                        result = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        if (cxt != null)
                            cxt.Dispose();
                    }
                }
            }
            return result;
        }

        public void InsertOrUpdateContactXero(string typeAction, SupplierModels model, string SupplierId)
        {
            var xeroInfo = Commons.GetIntegrateInfoWithComId(model.CompanyId);
            if (xeroInfo != null)
            {
                SupplierXeroFactory _facSupplier = new SupplierXeroFactory();
                var modelsXero = new SupplierXeroModels()
                {
                    AppRegistrationId = xeroInfo.ThirdPartyID,
                    StoreId = xeroInfo.IPAddress,

                    Contact = new ContactModels
                    {
                        ContactID = typeAction.Equals("update") ? model.Id : "",
                        Name = model.Name,
                        EmailAddress = model.Email,
                        BankAccountDetails = "",
                        IsSupplier = true,
                        IsCustomer = false,
                        HasAttachments = false,
                        Addresses = new List<AddressesModels>
                                    {
                                        new AddressesModels
                                        {
                                            AddressType = 1,
                                            City = model.City,
                                            Country = model.Country,
                                            PostalCode = model.ZipCode,
                                            Region = "",
                                        }
                                    },
                        Phones = new List<PhonesModels>
                                    {
                                        new PhonesModels
                                        {
                                            PhoneType = 1,
                                            PhoneNumber = model.Phone1,
                                            PhoneAreaCode = "",
                                            PhoneCountryCode = "",
                                        },
                                        new PhonesModels
                                        {
                                            PhoneType = 1,
                                            PhoneNumber = model.Phone2,
                                            PhoneAreaCode = "",
                                            PhoneCountryCode = "",
                                        }
                                    },
                        UpdatedDateUTC = DateTime.Now
                    },
                };
                var msgXero = string.Empty;
                var resultXero = _facSupplier.CreateOrUpdateSupplier(xeroInfo.ApiURL, modelsXero, SupplierId, ref msgXero);
            }
        }

        public bool Delete(string Id, ref string msg)
        {
            bool result = true;
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    if (IsCanDelete(Id))
                    {
                        var item = cxt.I_Supplier.Where(tb => tb.Id == Id).FirstOrDefault();
                        if (item != null)
                        {
                            item.Status = (int)Commons.EStatus.Deleted;
                            cxt.SaveChanges();
                        }
                    }
                    else
                    {
                        msg = "This supplier has been in used. Please deactivate it only.";
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    result = false;
                }
                finally
                {
                    if (cxt != null)
                        cxt.Dispose();
                }
            }
            return result;
        }

        //Get Data 
        public List<SupplierModels> GetData(string CompanyID = null, List<string> ListOrganizationId = null)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var lstResult = (from tb in cxt.I_Supplier
                                     where (tb.CompanyId.Equals(CompanyID) || CompanyID == null)
                                      && (tb.Status != (int)Commons.EStatus.Deleted && tb.Status != null)
                                     select new SupplierModels()
                                     {
                                         Id = tb.Id,
                                         CompanyId = tb.CompanyId,
                                         Name = tb.Name,
                                         Phone1 = tb.Phone1,
                                         Phone2 = tb.Phone2,
                                         Email = tb.Email,
                                         IsActived = tb.IsActived,
                                         Address = tb.Address
                                     }).ToList();
                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return new List<SupplierModels>();
                }
            }
        }

        // Updated 04172018
        public List<SupplierModels> GetDataByListCompany(List<string> ListCompanyId = null, bool? IsActived = null)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                List<SupplierModels> lstResult = new List<SupplierModels>();

                try
                {
                    if (ListCompanyId == null)
                    {
                        // Get listCompanyId from SESSION
                        if (System.Web.HttpContext.Current.Session["GetListStore_View_V1"] != null)
                        {
                            var listStores = (List<StoreModels>)HttpContext.Current.Session["GetListStore_View_V1"];
                            if (listStores != null && listStores.Any())
                            {
                                ListCompanyId = listStores.Select(ss => ss.CompanyId).Distinct().ToList();
                            }
                        }
                        else
                        {
                            var currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                            if (currentUser != null)
                            {
                                var listOrganization = currentUser.ListOrganizationId;
                                if (listOrganization != null && listOrganization.Any())
                                {
                                    CompanyFactory _companyFactory = new CompanyFactory();
                                    var listCompany = _companyFactory.GetListCompany(listOrganization);
                                    if (listCompany != null && listCompany.Any())
                                    {
                                        ListCompanyId = listCompany.Select(ss => ss.Value).Distinct().ToList();
                                    }
                                }
                            }
                        }
                    }

                    if (ListCompanyId != null && ListCompanyId.Any())
                    {
                        lstResult = (from tb in cxt.I_Supplier
                                     where ListCompanyId.Contains(tb.CompanyId)
                                      && (tb.Status != (int)Commons.EStatus.Deleted && tb.Status != null)
                                     orderby tb.Name
                                     select new SupplierModels()
                                     {
                                         Id = tb.Id,
                                         CompanyId = tb.CompanyId,
                                         Name = tb.Name,
                                         Phone1 = tb.Phone1,
                                         Phone2 = tb.Phone2,
                                         Email = tb.Email,
                                         IsActived = tb.IsActived,
                                         Address = tb.Address
                                     }).ToList();

                        if (lstResult != null && lstResult.Any() && IsActived.HasValue)
                        {
                            lstResult = lstResult.Where(ww => ww.IsActived == IsActived.Value).OrderBy(oo => oo.Name).ToList();
                        }
                    }

                    return lstResult;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return lstResult;
                }
            }
        }

        //Get Detail Data
        public SupplierModels GetDetail(string ID)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                try
                {
                    var model = (from tb in cxt.I_Supplier
                                 where tb.Id == ID && tb.Status != (int)Commons.EStatus.Deleted
                                 select new SupplierModels()
                                 {
                                     Id = tb.Id,
                                     Name = tb.Name,
                                     IsActived = tb.IsActived,
                                     Phone1 = tb.Phone1,
                                     Email = tb.Email,
                                     Country = tb.Country,
                                     CompanyId = tb.CompanyId,
                                     ZipCode = tb.ZipCode,
                                     Address = tb.Address,
                                     City = tb.City,
                                     Phone2 = tb.Phone2,
                                     Fax = tb.Fax,
                                     ContactInfo = tb.ContactInfo,
                                 }).FirstOrDefault();
                    return model;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    return null;
                }
            }
        }

        //Export Supplier
        public ResultModels Export(ref IXLWorksheet wsSupplier, ref IXLWorksheet wsSupplierIngerdient, List<string> lstCompanyId, List<SelectListItem> lstCompany)
        {
            var result = new ResultModels();
            try
            {
                using (var cxt = new NuWebContext())
                {

                    string[] lstHeaders = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier Name"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Address"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("City"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Country"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Zip Code"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Phone 1"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Phone 2"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Fax"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Email"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Contact Info"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("IsActive"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Company Name")
                    };
                    var lstData = (from s in cxt.I_Supplier
                                   where lstCompanyId.Contains(s.CompanyId) && s.Status != (int)Commons.EStatus.Deleted
                                   select new SupplierModels()
                                   {
                                       Id = s.Id,
                                       Name = s.Name,
                                       Address = s.Address,
                                       City = s.City,
                                       Country = s.Country,
                                       ZipCode = s.ZipCode,
                                       Phone1 = s.Phone1,
                                       Phone2 = s.Phone2,
                                       Fax = s.Fax,
                                       Email = s.Email,
                                       ContactInfo = s.ContactInfo,
                                       IsActived = s.IsActived,
                                       IngredientId = s.CompanyId,
                                       CompanyId = s.CompanyId
                                   }).ToList();
                    int row = 1, countSupplierIndex = 1, countSupplierIngIndex = 1;
                    //add header to excel file
                    for (int i = 1; i <= lstHeaders.Length; i++)
                        wsSupplier.Cell(row, i).Value = lstHeaders[i - 1];
                    int cols = lstHeaders.Length;
                    row = 2;
                    List<ExportIngSupplier> lstSupplierIng = new List<ExportIngSupplier>();
                    if (lstData != null && lstData.Count > 0)
                    {
                        foreach (var item in lstData)
                        {
                            var company = lstCompany.Where(x => x.Value.Equals(item.CompanyId)).FirstOrDefault();
                            wsSupplier.Cell("A" + row).Value = countSupplierIndex;
                            wsSupplier.Cell("B" + row).Value = item.Name;
                            wsSupplier.Cell("C" + row).Value = item.Address;
                            wsSupplier.Cell("D" + row).Value = item.City;
                            wsSupplier.Cell("E" + row).Value = item.Country;
                            wsSupplier.Cell("F" + row).Value = "'" + item.ZipCode;
                            wsSupplier.Cell("G" + row).Value = "'" + item.Phone1;
                            wsSupplier.Cell("H" + row).Value = "'" + item.Phone2;
                            wsSupplier.Cell("I" + row).Value = "'" + item.Fax;
                            wsSupplier.Cell("J" + row).Value = item.Email;
                            wsSupplier.Cell("K" + row).Value = item.ContactInfo;
                            wsSupplier.Cell("L" + row).Value = item.IsActived ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TRUE")
                                                : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("FALSE");
                            wsSupplier.Cell("M" + row).Value = company == null ? "" : company.Text;

                            /*Get List Ingredient Supplier*/
                            var listIngSupplier = (from IS in cxt.I_Ingredient_Supplier
                                                   from I in cxt.I_Ingredient
                                                   where IS.SupplierId.Equals(item.Id) && IS.IngredientId.Equals(I.Id)
                                                        //&& I.IsActive 
                                                        && I.Status != (int)Commons.EStatus.Deleted
                                                        && IS.IsActived
                                                   select new ExportIngSupplier
                                                   {
                                                       IngredientCode = I.Code,
                                                       IngredientName = I.Name,
                                                   }).ToList();

                            foreach (var SupplierIng in listIngSupplier)
                            {
                                ExportIngSupplier etIngSupplier = new ExportIngSupplier()
                                {
                                    Index = countSupplierIngIndex,
                                    SupplierIndex = countSupplierIndex,
                                    Name = item.Name,

                                    IngredientName = SupplierIng.IngredientName,
                                    IngredientCode = SupplierIng.IngredientCode,
                                };
                                lstSupplierIng.Add(etIngSupplier);
                                countSupplierIngIndex++;
                            }
                            //=====
                            row++;
                            countSupplierIndex++;
                        }
                    }
                    FormatExcelExport(wsSupplier, row, cols);

                    //==============================
                    string[] lstHeader = new string[] {
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier Index"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier Name"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredients Code"),
                        _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredients Name")
                    };
                    row = 1;
                    for (int i = 1; i <= lstHeader.Length; i++)
                        wsSupplierIngerdient.Cell(row, i).Value = lstHeader[i - 1];
                    int colss = lstHeader.Length;
                    row = 2;
                    foreach (var item in lstSupplierIng)
                    {
                        wsSupplierIngerdient.Cell("A" + row).Value = item.Index;
                        wsSupplierIngerdient.Cell("B" + row).Value = item.SupplierIndex;
                        wsSupplierIngerdient.Cell("C" + row).Value = item.Name;
                        wsSupplierIngerdient.Cell("D" + row).Value = item.IngredientCode;
                        wsSupplierIngerdient.Cell("E" + row).Value = item.IngredientName;
                        row++;
                    }
                    FormatExcelExport(wsSupplierIngerdient, row, colss);
                    result.IsOk = true;
                }
            }
            catch (Exception ex)
            {
                result.IsOk = false;
                result.Message = ex.Message;
                _logger.Error(ex);
            }
            return result;
        }

        //Import Supplier
        public StatusResponse Import(string filePath, string userName, out int totalRowExcel, List<string> ListCompanyId, ref ImportModel importModel, ref string msg)
        {
            totalRowExcel = 0;
            StatusResponse Response = new StatusResponse();
            using (NuWebContext cxt = new NuWebContext())
            {
                using (var transaction = cxt.Database.BeginTransaction())
                {
                    DataTable dtSupplier = ReadExcelFile(filePath, "Supplier");
                    DataTable dtIngSupplier = ReadExcelFile(filePath, "Ingredients_Supplier");

                    string tmpExcelPath = System.Web.HttpContext.Current.Server.MapPath("~/ImportExportTemplate") + "/SupplierImportTemplate.xlsx";
                    DataTable dtTmpSupplier = ReadExcelFile(@tmpExcelPath, "Supplier");
                    DataTable dtTmpSupplierIng = ReadExcelFile(@tmpExcelPath, "Ingredients_Supplier");

                    if (dtSupplier.Columns.Count != dtTmpSupplier.Columns.Count)
                    {
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                        return Response;
                    }
                    if (dtIngSupplier.Columns.Count != dtTmpSupplierIng.Columns.Count)
                    {
                        msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgDoesNotMatchFileExcel);
                        return Response;
                    }
                    //========
                    var listcountry = cxt.I_Country.Where(x => x.IsActived).ToList();
                    var listing = cxt.I_Ingredient.Where(x => x.IsActive).ToList();

                    bool flagInsert = true;
                    string msgError = "";

                    ImportItem itemErr = null;

                    List<SupplierModels> Models = new List<SupplierModels>();
                    SupplierModels SupModel = null;
                    foreach (var company in ListCompanyId)
                    {
                        foreach (DataRow item in dtSupplier.Rows)
                        {

                            flagInsert = true;
                            msgError = "";

                            if (item[0].ToString().Equals(""))
                                continue;

                            int index = int.Parse(item[0].ToString());
                            //var Country = listcountry.Where(x => x.FullName.Trim().ToLower().Equals(item[4].ToString().Trim().ToLower())).FirstOrDefault();
                            //if (Country == null)
                            //{
                            //    flagInsert = false;
                            //    msgError += "<br/>Country is not exsits!";
                            //}
                            //=======================
                            SupModel = new SupplierModels();
                            string SupplierId = Guid.NewGuid().ToString();
                            SupModel.Id = SupplierId;
                            SupModel.CompanyId = company;
                            SupModel.Index = index.ToString();
                            SupModel.Name = item[1].ToString();
                            SupModel.Address = item[2].ToString();
                            SupModel.City = item[3].ToString();
                            SupModel.Country = item[4].ToString().Trim();//Country == null ? "" : Country.Id;
                            SupModel.ZipCode = item[5].ToString();
                            SupModel.Phone1 = item[6].ToString();
                            SupModel.Phone2 = item[7].ToString();
                            SupModel.Fax = item[8].ToString();
                            SupModel.Email = item[9].ToString();
                            SupModel.ContactInfo = item[10].ToString();
                            SupModel.IsActived = string.IsNullOrEmpty(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item[11].ToString())) ? false
                                                    : bool.Parse(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item[11].ToString()).ToLower().Equals("true").ToString()) ? true : false;

                            /*List Ingredient Supplier*/
                            List<Ingredients_SupplierModel> lstSupplierIng = new List<Ingredients_SupplierModel>();
                            DataRow[] IngSupplier = dtIngSupplier.Select("[" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier Index") + "] = " + index + "");
                            foreach (DataRow ingsupplier in IngSupplier)
                            {
                                var Ingredient = listing.Where(x => x.Code.ToLower().Equals(ingsupplier[3].ToString().ToLower())).FirstOrDefault();
                                if (string.IsNullOrEmpty(ingsupplier[1].ToString()))
                                {
                                    flagInsert = false;
                                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier Index is required");
                                }
                                //====
                                if (string.IsNullOrEmpty(ingsupplier[3].ToString()))
                                {
                                    flagInsert = false;
                                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Code is required");
                                }
                                else if (Ingredient == null)
                                {
                                    flagInsert = false;
                                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient is not exsits");
                                }
                                if (string.IsNullOrEmpty(ingsupplier[4].ToString()))
                                {
                                    flagInsert = false;
                                    msgError += "<br/>" + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Ingredient Name is required");
                                }
                                if (flagInsert)
                                {
                                    Ingredients_SupplierModel SupplierIngredient = new Ingredients_SupplierModel()
                                    {
                                        IngredientId = Ingredient.Id
                                    };
                                    lstSupplierIng.Add(SupplierIngredient);
                                }
                            }
                            SupModel.ListSupIng = lstSupplierIng;
                            //======================
                            if (flagInsert)
                            {
                                Models.Add(SupModel);
                            }
                            else
                            {
                                itemErr = new ImportItem();
                                itemErr.Name = SupModel.Name;
                                itemErr.ListFailStoreName.Add("");
                                itemErr.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + SupModel.Index + msgError);
                                importModel.ListImport.Add(itemErr);
                            }
                        }
                    }
                    //=================
                    Response.Status = true;
                    try
                    {
                        List<I_Supplier> lstSave = new List<I_Supplier>();
                        I_Supplier item = null;
                        foreach (var model in Models)
                        {
                            item = new I_Supplier();
                            item.Id = model.Id;
                            item.CompanyId = model.CompanyId;
                            item.Name = model.Name;
                            item.Address = model.Address;
                            item.City = model.City;
                            item.Country = model.Country;
                            item.ZipCode = model.ZipCode;
                            item.Phone1 = model.Phone1;
                            item.Phone2 = model.Phone2;
                            item.Fax = model.Fax;
                            item.Email = model.Email;
                            item.ContactInfo = model.ContactInfo;

                            item.CreatedBy = userName;
                            item.CreatedDate = DateTime.Now;
                            item.ModifierDate = DateTime.Now;
                            item.ModifierBy = userName;
                            item.IsActived = model.IsActived;

                            item.Status = (int)Commons.EStatus.Actived;

                            lstSave.Add(item);
                        }
                        cxt.I_Supplier.AddRange(lstSave);

                        //Supplier Ingredient
                        foreach (var model in Models)
                        {
                            /*Detail*/
                            //Insert Ingredient Supplier
                            List<I_Ingredient_Supplier> LstSupplierIng = new List<I_Ingredient_Supplier>();
                            foreach (var SupIng in model.ListSupIng)
                            {
                                LstSupplierIng.Add(new I_Ingredient_Supplier
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IngredientId = SupIng.IngredientId,
                                    SupplierId = model.Id,
                                    CreatedBy = userName,
                                    CreatedDate = DateTime.Now,
                                    ModifierDate = DateTime.Now,
                                    ModifierBy = userName,
                                    IsActived = model.IsActived,
                                });
                            }
                            if (LstSupplierIng.Count > 0)
                            {
                                cxt.I_Ingredient_Supplier.AddRange(LstSupplierIng);
                            }
                            /*End Detail*/
                        }
                        cxt.SaveChanges();
                        transaction.Commit();

                        if (importModel.ListImport.Count == 0)
                        {
                            ImportItem impItem = new ImportItem();
                            impItem.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Supplier");
                            impItem.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Supplier Successful"));
                            importModel.ListImport.Add(impItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.Error(ex);
                    }
                }
            }
            return Response;
        }

        public ResultModels EnableActive(List<string> lstId, bool active)
        {
            //if (!active)
            //{
            //    lstId = ListIngInActive(lstId);
            //}
            ResultModels data = new ResultModels();
            using (var cxt = new NuWebContext())
            {
                var lstObj = cxt.I_Supplier.Where(ww => lstId.Contains(ww.Id)).ToList();
                if (lstObj != null && lstObj.Count > 0)
                {
                    lstObj.ForEach(ss => ss.IsActived = active);
                    cxt.SaveChanges();
                    data.IsOk = true;
                }
                else
                {
                    data.IsOk = false;
                    data.Message = "Not found!";
                }
            }
            return data;
        }

        private List<string> ListIngInActive(List<string> lstId)
        {
            List<string> lstIdInActive = lstId;
            using (var cxt = new NuWebContext())
            {
                var listIng = cxt.I_Ingredient_Supplier.Where(aa => lstIdInActive.Contains(aa.IngredientId)).Select(x => x.SupplierId).ToList();
                listIng.AddRange(cxt.I_Purchase_Order.Where(aa => lstIdInActive.Contains(aa.SupplierId) /*&& aa.Status != (int)Commons.EStatus.Deleted*/).Select(x => x.SupplierId).ToList());
                listIng.AddRange(cxt.I_ReceiptNote.Where(aa => (lstIdInActive.Contains(aa.SupplierId)) /*&& aa.Status != (int)Commons.EStatus.Deleted*/).Select(x => x.SupplierId).ToList());
                //Remove
                lstIdInActive.RemoveAll(i => listIng.Contains(i));
                return lstIdInActive;
            }
        }

        private bool IsCanDelete(string id)
        {
            using (var cxt = new NuWebContext())
            {
                var isExists = cxt.I_Ingredient_Supplier.Any(x => x.SupplierId == id && x.IsActived == true);
                if (!isExists)
                {
                    isExists = cxt.I_Purchase_Order.Any(x => x.SupplierId == id);
                    if (!isExists)
                    {
                        isExists = cxt.I_ReceiptNote.Any(x => (x.SupplierId == id));
                    }
                }
                return !isExists;
            }
        }
    }
}


