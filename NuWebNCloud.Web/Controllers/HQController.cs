using NuWebNCloud.Shared;
using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Factory.Xero.Settings.Tax;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Models.Xero.Settings.Tax;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuWebNCloud.Web.Controllers
{
    public class HQController : Controller
    {
        public UserSession CurrentUser
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["User"] != null)
                    return (UserSession)System.Web.HttpContext.Current.Session["User"];
                else
                    return new UserSession();
            }
        }

        public HQController()
        {
            //Updated 04022018, for set css menu toggle
            ViewBag.IsFromNuPos = CurrentUser.IsFromNuPos;

            // Updated 04132018, for view selectlist store
            StoreFactory _storeFactory = new StoreFactory();
            var listStoreInfo = _storeFactory.GetListStore_View_V1();
            ViewBag.StoreID = new SelectList(listStoreInfo, "Id", "Name", "CompanyName", 1);
        }

        public void GetListStoreGroup()
        {
            var ListCompanyStore = GetListStoreRole();
            ViewBag.StoreID = new SelectList(ListCompanyStore, "Id", "Name", "CompanyName", 1);
        }

        public List<SelectListItem> GetListStore()
        {
            List<SelectListItem> ListStore = new List<SelectListItem>();
            try
            {
                StoreFactory _storeFactory = new StoreFactory();
                CompanyFactory _companyFactory = new CompanyFactory();
                List<string> listOrganizationId = new List<string>();
                if (CurrentUser != null)
                {
                    listOrganizationId = CurrentUser.ListOrganizationId;
                }
                _storeFactory = new StoreFactory();
                //_companyFactory = new CompanyFactory();
                ListStore = _storeFactory.GetListStore(listOrganizationId);
                //ListStore = _storeFactory.GetListStore(_companyFactory.GetListCompany(listOrganizationId));
                /*Editor by trongntn 06-01-2017*/
                ListStore = ListStore.Where(x => CurrentUser.ListStoreID.Contains(x.Value)).ToList();
            }
            catch (Exception)
            {
            }
            return ListStore;
        }

        public List<StoreModels> GetListStores()
        {
            List<StoreModels> ListStore = new List<StoreModels>();
            try
            {
                StoreFactory _storeFactory = new StoreFactory();                
                List<string> listOrganizationId = new List<string>();
                if (CurrentUser != null)
                {
                    listOrganizationId = CurrentUser.ListOrganizationId;
                }
                _storeFactory = new StoreFactory();
                //_companyFactory = new CompanyFactory();
                ListStore = _storeFactory.GetListStores(listOrganizationId);
                //ListStore = _storeFactory.GetListStore(_companyFactory.GetListCompany(listOrganizationId));
                /*Editor by trongntn 06-01-2017*/
                ListStore = ListStore.Where(x => CurrentUser.ListStoreID.Contains(x.Id)).ToList();
            }
            catch (Exception)
            {
            }
            return ListStore;
        }
        public List<StoreModels> GetListStoreRole()
        {
            List<StoreModels> ListStore = new List<StoreModels>();
            try
            {
                StoreFactory _storeFactory = new StoreFactory();
                CompanyFactory _companyFactory = new CompanyFactory();
                List<string> listOrganizationId = new List<string>();
                if (CurrentUser != null)
                {
                    listOrganizationId = CurrentUser.ListOrganizationId;
                }
                _storeFactory = new StoreFactory();
                ListStore = _storeFactory.GetListStoreRole(listOrganizationId);
                ListStore = ListStore.Where(x => CurrentUser.ListStoreID.Contains(x.Id)).ToList();
            }
            catch (Exception)
            {
            }
            return ListStore;
        }

        public List<SelectListItem> GetListStoreIntegration()
        {
            List<SelectListItem> ListStore = new List<SelectListItem>();
            try
            {
                var lStore = CurrentUser.listStore;
                var lstData = lStore.GroupBy(x => new { ID = x.ID, Name = x.Name }).OrderBy(x => x.Key.Name);
                foreach (var item in lstData)
                {
                    ListStore.Add(new SelectListItem()
                    {
                        Value = item.Key.ID.ToString(),
                        Text = item.Key.Name
                    });
                }


            }
            catch (Exception)
            {
            }
            return ListStore;
        }

        public List<StoreModels> GetListStoreForTransfer()
        {
            List<StoreModels> ListStore = new List<StoreModels>();
            try
            {
                StoreFactory _storeFactory = new StoreFactory();
                CompanyFactory _companyFactory = new CompanyFactory();
                List<string> listOrganizationId = new List<string>();
                if (CurrentUser != null)
                {
                    listOrganizationId = CurrentUser.ListOrganizationId;
                }
                _storeFactory = new StoreFactory();
                _companyFactory = new CompanyFactory();
                ListStore = _storeFactory.GetListStoreForTranfer(listOrganizationId);
                if (ListStore == null)
                    ListStore = new List<StoreModels>();
                //ListStore = ListStore.Where(x => CurrentUser.ListStoreID.Contains(x.Value)).ToList();
            }
            catch (Exception)
            {
            }
            return ListStore;
        }
        public List<SelectListItem> GetListCompany()
        {
            List<SelectListItem> ListCompany = new List<SelectListItem>();
            try
            {
                CompanyFactory _companyFactory = new CompanyFactory();
                List<string> listOrganization = new List<string>();
                if (CurrentUser != null)
                {
                    listOrganization = CurrentUser.ListOrganizationId;
                }
                _companyFactory = new CompanyFactory();
                ListCompany = _companyFactory.GetListCompany(listOrganization);
            }
            catch (Exception)
            {

            }
            return ListCompany;
        }

        public List<SelectListItem> ListTaxXero(string StoreId = "", string AppId = "",string ApiURL="")
        {
            //get list tax from xero
            List<SelectListItem> Tax = new List<SelectListItem>();
            TaxXeroFactory _factoryTaxXero = new TaxXeroFactory();
            try
            {
                TaxXeroRequestModels item = new TaxXeroRequestModels();
                item.StoreId = StoreId; //Commons.XeroStoreId;
                item.AppRegistrationId = AppId; //Commons.XeroRegistrationAppId;
                item.ApiURL = ApiURL;
                var data = _factoryTaxXero.GetTaxXero(item);
                if(data != null && data.Any())
                {
                    foreach(var tax in data)
                    {
                        Tax.Add(new SelectListItem
                        {
                            Value = tax.TaxType,
                            Text = tax.Name
                        });
                    }
                }
            }
            catch (Exception ex) { }
            return Tax;
        }
    }
}