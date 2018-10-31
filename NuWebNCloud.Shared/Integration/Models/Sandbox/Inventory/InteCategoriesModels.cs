using NuWebNCloud.Shared.Factory;
using NuWebNCloud.Shared.Models.Sandbox.Inventory;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory
{
    public class InteCategoriesModels : SBInventoryBaseModel
    {
        public string Index { get; set; }
        public string ID { get; set; }
        public string StoreID { get; set; }

        public string StoreName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int Sequence { get; set; }

        public string Name { get; set; }

        public int TotalProducts { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }

        public string ParentID { get; set; }
        public string ParentName { get; set; }

        public byte Status { get; set; }
        public string GLAccountCode { get; set; }
        public bool IsGiftCard { get; set; }
        public bool IsIncludeNetSale { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }

        public bool IsShowInReservation { get; set; }

        public string ProductTypeID { get; set; }
        public string ProductTypeName { get; set; }
        public int Type { get; set; }

        public List<SelectListItem> ListProductType { get; set; }

        public List<InteCategoriesModels> ListChild { get; set; }
        public List<SelectListItem> ListStore { get; set; }
        public List<ItemOnStore> ListItemOnStores { get; set; }
        public List<CategoryOnStoreWebDTO> ListCategoryOnStore { get; set; }
        public bool IsSelected { get; set; }

        public InteCategoriesModels()
        {
            ListProductType = new List<SelectListItem>()
            {
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dish"),Value=Commons.EProductType.Dish.ToString("d") },
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Modifier"),Value=Commons.EProductType.Modifier.ToString("d")},
                new SelectListItem() {Text=_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Set"),Value=Commons.EProductType.SetMenu.ToString("d")}
            };
            ListStore = new List<SelectListItem>();
            ListItemOnStores = new List<ItemOnStore>();
            ListCategoryOnStore = new List<CategoryOnStoreWebDTO>();
        }
        public void GetListProductType()
        {
            ListProductType = new List<SelectListItem>();

            ProductTypeFactory ProTypeFactory = new ProductTypeFactory();
            var listPT = ProTypeFactory.GetListProductType();
            foreach (var item in listPT)
            {
                if (item.Name.ToLower().Equals("promotion") || item.Name.ToLower().Equals("discount"))
                    continue;
                ListProductType.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.ID
                });
            }
        }
    }
    public class ItemOnStore
    {
        public string StoreID { get; set; }
        public int OffSet { get; set; }
        public int State { get; set; }
        public string StoreName { get; set; }
        public int Sequence { get; set; }
        public bool IsShowInReservation { get; set; }
        public bool IsShowInKiosk { get; set; }
        public int IsCheckDisabled { get; set; }
    }
}
