using NuWebNCloud.Shared.Factory.Ingredients;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class ReceiptNoteModels
    {
        public string Id { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public string ReceiptNo { get; set; }
        public int Status { get; set; }

        public string ListPONo { get; set; }
        public string PONo { get; set; }

        public DateTime ReceiptDate { get; set; }
        public string ReceiptBy { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

        public List<ReceiptNoteIngredient> ListItem { get; set; }
        public List<string> ListStores { get; set; }

        public List<SelectListItem> ListSupplier { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }

        public List<ReturnNoteModels> ListReturnNote { get; set; }
        public List<PurchaseOrderModels> ListPurchaseOrder { get; set; }

        //Check type Create Receipt Note 
        public bool IsPurchaseOrder { get; set; }
        /*Export*/
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public ReceiptNoteModels()
        {
            ReceiptDate = DateTime.Now;

            FromDate = DateTime.Now;
            ToDate = DateTime.Now;

            ListReturnNote = new List<ReturnNoteModels>();
            ListPurchaseOrder = new List<PurchaseOrderModels>();

            ListSupplier = new List<SelectListItem>();

            ListItem = new List<ReceiptNoteIngredient>();

            IsPurchaseOrder = true;
        }

        public void GetListSupplierFromCompnay(List<string> ListCompanyId)
        {
            SupplierFactory _SupplierFactory = new SupplierFactory();
            //var dataSupplier = _SupplierFactory.GetData().Where(x => x.IsActived && ListCompanyId.Contains(x.CompanyId)).ToList();
            var dataSupplier = _SupplierFactory.GetDataByListCompany(ListCompanyId, true); // Updated 04172018

            foreach (var item in dataSupplier)
            {
                ListSupplier.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id
                });
            }
        }
    }

    public class ReceiptNoteViewModels
    {
        public string StoreID { get; set; }

        public List<string> ListSupplierId { get; set; }
        public List<SelectListItem> ListSupplier { get; set; }

        public DateTime? ReceiptNoteDate { get; set; }

        public List<ReceiptNoteModels> ListItem { get; set; }
        public ReceiptNoteViewModels()
        {
            ListItem = new List<ReceiptNoteModels>();
            //ReceiptNoteDate = DateTime.Now;

            ListSupplier = new List<SelectListItem>();
            //SupplierFactory _SupplierFactory = new SupplierFactory();
            ////var dataSupplier = _SupplierFactory.GetData().Where(x => x.IsActived).ToList();
            //var dataSupplier = _SupplierFactory.GetDataByListCompany(null, true); // Updated 04172018
            //foreach (var item in dataSupplier)
            //{
            //    ListSupplier.Add(new SelectListItem
            //    {
            //        Text = item.Name,
            //        Value = item.Id
            //    });
            //}
        }
    }

    public class ReceiptNoteViewDetailModels
    {
        public ReceiptNoteModels ReceiptNote { get; set; }
        public List<ReceiptNoteDetailModels> ListItem { get; set; }
    }

    public class ReceiptNoteIngredient
    {
        public bool IsSelect { get; set; }
        public string IngredientCode { get; set; }
        public string IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Description { get; set; }
        public string BaseUOM { get; set; }
        public double BaseReceivingQty { get; set; }
        public int OffSet { get; set; }
        public double Qty { get; set; }
        public int Delete { get; set; }
        //==update
        public string BaseUOMId { get; set; }
        public List<SelectListItem> ListUOM { get; set; }
        public double BaseUsage { get; set; }

        public ReceiptNoteIngredient()
        {
            ListUOM = new List<SelectListItem>();
        }
    }

    public class ReceiptNoteIngredientViewModels
    {
        public int Type { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string StoreId { get; set; }
        public List<ReceiptNoteIngredient> ListItem { get; set; }

        public ReceiptNoteIngredientViewModels()
        {
            ListItem = new List<ReceiptNoteIngredient>();
        }
    }
}
