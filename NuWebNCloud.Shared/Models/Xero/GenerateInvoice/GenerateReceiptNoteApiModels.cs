using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Xero.GenerateInvoice
{
    public class GenerateReceiptNoteApiModels
    {
        /// <summary>
        /// AppRegistrationId => "6aaee02a-5dc0-466a-9352-5e9e279c1fe2";
        /// </summary>
        public string AppRegistrationId { get; set; }
        /// <summary>
        /// StoreId => NS-XERO-INTEGRATION
        /// </summary>
        public string StoreId { get; set; }
        /// <summary>
        /// ApiURL => http://nupos.freeddns.org/xero/2.0/
        /// </summary>
        public string ApiURL { get; set; }

        /// <summary>
        /// SupplierName => Contact
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// Code => Reference
        /// </summary>
        public string Code { get; set; }

        public List<ItemIngredient> ListItem { get; set; }
        /// <summary>
        /// value TaxType from [TaxExempt = 1 | Inclusive = 2 | AddOn = 3]
        /// </summary>
        //public int TaxType { get; set; }

        /// <summary>
        /// Get value from EInvoiceType = ACCPAY[AccountsPayable]
        /// </summary>
        public byte InvoiceType { get; set; }

        /// <summary>
        /// Get value from EInvoiceStatus = follow the setting of “Send bill as”
        /// </summary>
        public byte InvoiceStatus { get; set; }
        /// <summary>
        /// Get value from ELineAmountType
        /// </summary>
        public byte LineAmountType { get; set; }

        /// <summary>
        /// Due date = if in xero - Settings - Invoice Settings - Default Settings - Bills Default Due Date has value, 
        /// Due date = current date + the setting, if not, current date
        /// </summary>
        public DateTime DueDate { get; set; }
        /// <summary>
        /// ClosingDatetime = PO Time = Current Date
        /// </summary>
        public DateTime ClosingDatetime { get; set; }

        public GenerateReceiptNoteApiModels()
        {
            ListItem = new List<ItemIngredient>();
            DueDate = DateTime.Now;
            ClosingDatetime = DateTime.Now;
        }
    }
}
