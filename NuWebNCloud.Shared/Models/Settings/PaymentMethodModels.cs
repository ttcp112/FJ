using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Validation;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NuWebNCloud.Shared.Models.Settings
{
    public class PaymentMethodModels
    {
        public string Index { get; set; } /* Index in excel file */

        public string ID { get; set; }
        //[Required]
        public string Name { get; set; }

        public string ParentName { get; set; }
        public bool IsActive { get; set; }
        public bool IsHasConfirmCode { get; set; }
        public string GLAccountCode { get; set; }

        //[Required(ErrorMessage = "Please choose Store")]
        public string StoreID { get; set; }

        public string StoreName { get; set; }
        public byte Code { get; set; }
        public string ParentId { get; set; }

        [_AttributeForLanguage("The Number Of Copies field is required.")]
        [_AttributeForLanguageRange(0, int.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public int NumberOfCopies { get; set; }
        [_AttributeForLanguage("The Fixed Amount field is required.")]
        public double FixedAmount { get; set; }
        public bool IsGiveBackChange { get; set; }
        public bool IsAllowKickDrawer { get; set; }
        public bool IsIncludeOnSale { get; set; }
        public bool IsShowOnPos { get; set; }
        [_AttributeForLanguage("Sequence field is required.")]
        public int Sequence { get; set; }
        public string ImageURL { get; set; }
        [DataType(DataType.Upload)]
        [FileTypes("jpeg,jpg,png")]
        public HttpPostedFileBase PictureUpload { get; set; }
        public byte[] PictureByte { get; set; }
        public byte[] photoByte { get; set; }
        public string ColorCode { get; set; }
        public List<PaymentMethodModels> ListChild { get; set; }
        //=====================
        public string CreatedUser { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int Mode { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        //==============
        [DataType(DataType.Upload)]
        [FileSize(3072000)]
        [FileTypes("xls,xlsx")]
        public HttpPostedFileBase ExcelUpload { get; set; }

        public List<string> ListStores { get; set; }

        public int OffSet { get; set; }
        public byte Status { get; set; }

        public PaymentMethodModels()
        {
            IsIncludeOnSale = true;
            IsShowOnPos = true;
            Name = "";
        }
    }

    public class PaymentMethodViewModels
    {
        public string StoreID { get; set; }
        public List<PaymentMethodModels> ListItem { get; set; }
        public PaymentMethodViewModels()
        {
            ListItem = new List<PaymentMethodModels>();
        }
    }
}
