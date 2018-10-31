using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class SetMenuImportModel
    {
        public List<SetMenuImportItem> ListSetMenu { get; set; }

        public List<TabImportItem> ListTab { get; set; }

        public List<DishTabImportItem> ListDish { get; set; }
    }

    public class SetMenuImportItem
    {
        public int SetMenuIndex { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public string SetMenuCode { get; set; }

        public bool IsCheckQuantity { get; set; }

        public bool IsAllowDiscount { get; set; }

        public string ShownDescription { get; set; }

        public bool ShowShownDes { get; set; }

        public string PrintDescription { get; set; }

        public bool ShownPrintDes { get; set; }

        public double DefaultPrice { get; set; }

        public double Cost { get; set; }

        public decimal? Quantity { get; set; }

        public int Limit { get; set; }

        public bool IsActive { get; set; }

        public bool IsOpenPrice { get; set; }

        public bool IsPrintOnCheck { get; set; }

        public DateTime ExpiredDate { get; set; }

        public bool IsServiceCharge { get; set; }

        public double PercentServiceCharge { get; set; }

        public bool ShowInReservationNQuee { get; set; }

        public string StoreName { get; set; }

        public string ImgUrl { get; set; }

        public string URL { get; set; }

        public InforImportModel Infor { get; set; }

        public int RowCount { get; set; }

        public string Picture { get; set; }

        public SetMenuImportItem()
        {
            Infor = new InforImportModel();
        }
    }

    public class TabImportItem
    {
        public int TabIndex { get; set; }

        public int Seq { get; set; }

        public int SetMenuIndex { get; set; }

        public string SetMenuName { get; set; }

        public string Name { get; set; }

        public string DisplayMessage { get; set; }

        public int Quantity { get; set; }

        public InforImportModel Infor { get; set; }

        public int RowCount { get; set; }

        public TabImportItem()
        {
            Infor = new InforImportModel();
        }
    }

    public class DishTabImportItem
    {
        public int DishIndex { get; set; }

        public int Seq { get; set; }

        public int TabIndex { get; set; }

        public string Name { get; set; }

        public double ExtraPrice { get; set; }

        public string Printer { get; set; }

        public InforImportModel Infor { get; set; }

        public int RowCount { get; set; }

        public DishTabImportItem()
        {
            Infor = new InforImportModel();
        }
    }
}
