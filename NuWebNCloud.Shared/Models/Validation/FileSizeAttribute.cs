using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NuWebNCloud.Shared.Models.Validation
{
    public class FileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSize;

        public FileSizeAttribute(int maxSize)
        {
            _maxSize = maxSize;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            return _maxSize > (value as HttpPostedFileBase).ContentLength;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(Commons._MsgAllowedSizeImg), _maxSize);
        }
    }
}
