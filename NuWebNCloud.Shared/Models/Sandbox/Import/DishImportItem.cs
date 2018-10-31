using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class DishImportItem
    {
        #region Import
        public int DishIndex { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }

        public string DishCode { get; set; }

        public bool IsAllowDiscount { get; set; }

        public bool IsCheckQuantity { get; set; }

        public string ShownDescription { get; set; }

        public bool ShowShownDes { get; set; }

        public string PrintDescription { get; set; }

        public bool ShownPrintDes { get; set; }

        public double Cost { get; set; }

        public int Unit { get; set; }

        public string Measure { get; set; }

        public double DefaultPrice { get; set; }

        public decimal? Quantity { get; set; }

        public int Limit { get; set; }

        public string CategoryName { get; set; }

        public bool IsActive { get; set; }

        public bool IsOpenPrice { get; set; }

        public bool IsPrintOnCheck { get; set; }

        public DateTime ExpiredDate { get; set; }

        public bool IsServiceCharge { get; set; }

        public double PercentServiceCharge { get; set; }

        public bool ForceModifier { get; set; }

        public string StoreName { get; set; }

        public string ImageUrl { get; set; }

        public string Printer { get; set; }

        public string DefaultStatusMapping
        {
            get
            {
                return _defaultStatus;
            }
            set
            {
                _defaultStatus = value;
                //if (_defaultStatus.ToLower() == Commons.EItemState.PendingStatus.ToString())
                //{
                //    DefaultStatus = 1;
                //}
                //else if (_defaultStatus.ToLower() == Constants.ReadyStatus.ToString())
                //{
                //    DefaultStatus = 2;
                //}
                //else
                //{
                //    DefaultStatus = 0;
                //}
            }
        }

        public bool IsComingSoon { get; set; }  //Coming Soon
        public bool IsShowMessage { get; set; } //Show Kisok Message
        public string Info { get; set; }  //Dish Information
        public string Message { get; set; }  //Message

        public string URL { get; set; }

        public string Picture { get; set; }

        public string ColorCode { get; set; }



        #endregion

        public int DefaultStatus { get; set; }

        private string _defaultStatus;

        public InforImportModel Infor { get; set; }

        public int RowCount { get; set; }

        public DishImportItem()
        {
            Infor = new InforImportModel();

            // default value for colorcode and defaultStatus
            // db was changed
            ColorCode = string.Empty;
        }
    }
}
