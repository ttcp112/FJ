using NuWebNCloud.Shared.Factory.Ingredients;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models.Ingredients
{
    public class IngredientUOMModels
    {
        public string Id { get; set; }
        public string IngredientId { get; set; }

        //[Required(ErrorMessage = "Please choose uom")]
        public string UOMId { get; set; }
        public string UOMName { get; set; }
        public List<SelectListItem> ListUOM { get; set; }

        public double BaseUOM { get; set; }

        [Required(ErrorMessage = "Receiving Quantity field is required")]
        public double ReceivingQty { get; set; }

        public bool IsActived { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }

        public int OffSet { get; set; }
        public byte Status { get; set; }

        public IngredientUOMModels()
        {
            ReceivingQty = 1;
            ListUOM = new List<SelectListItem>();
            UnitOfMeasureFactory _UOMFactory = new UnitOfMeasureFactory();
            if (System.Web.HttpContext.Current.Session["User"] != null)
            {
                var user = (UserSession)System.Web.HttpContext.Current.Session["User"];
                var lstItem = _UOMFactory.GetData(user.ListOrganizationId).Where(x => x.IsActive).ToList();
                if (lstItem != null)
                {
                    foreach (UnitOfMeasureModel uom in lstItem)
                        ListUOM.Add(new SelectListItem
                        {
                            Text = uom.Name,
                            Value = uom.Id
                        });
                }
            }
        }
    }
}
