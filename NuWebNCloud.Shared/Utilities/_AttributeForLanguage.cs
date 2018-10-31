using NuWebNCloud.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System;
using System.Configuration;

namespace NuWebNCloud.Shared.Utilities
{
    public class _AttributeForLanguage : RequiredAttribute
    {
        public static UserSession CurrentUser
        {
            get
            {
                if (System.Web.HttpContext.Current.Session != null && System.Web.HttpContext.Current.Session["User"] != null)
                    return (UserSession)System.Web.HttpContext.Current.Session["User"];
                else
                    return new UserSession();
            }
        }

        private string _displayName;

        public _AttributeForLanguage(string key)
        {
            ErrorMessageResourceName = key;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _displayName = validationContext.DisplayName;
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            // LanguageService.Instance.Translate(ErrorMessageResourceName);
            var msg = CurrentUser.GetLanguageTextFromKey(ErrorMessageResourceName);
            return string.Format(msg, _displayName);
        }
    }

    public class _AttributeForLanguageRange : RangeAttribute
    {
        private string _displayName;
        public _AttributeForLanguageRange(int minimum, int maximum) : base(minimum, maximum)
        {
        }

        public _AttributeForLanguageRange(double minimum, double maximum) : base(minimum, maximum)
        {
        }

        public _AttributeForLanguageRange(Type type, string minimum, string maximum) : base(type, minimum, maximum)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _displayName = validationContext.DisplayName;
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(this.ErrorMessage);
            return string.Format(msg, _displayName);
        }
    }

    public class _AttributeForLanguageRegularExpression : RegularExpressionAttribute
    {
        private string _displayName;
        public _AttributeForLanguageRegularExpression(string pattern) : base(pattern)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _displayName = validationContext.DisplayName;
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(this.ErrorMessage);
            return string.Format(msg, _displayName);
        }
    }

    public class _AttributeForLanguageStringLength : StringLengthAttribute
    {
        private string _displayName;

        public _AttributeForLanguageStringLength(int maximumLength) : base(maximumLength)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _displayName = validationContext.DisplayName;
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(this.ErrorMessage);
            return string.Format(msg, _displayName);
        }
    }

    /*For Format DateTime*/
    public class DynamicDisplayFormatAttribute : DisplayFormatAttribute
    {
        /*
            +Edit file [/js/daterangpicker.js] => [this.format]
            <add key="FormatDate" value="MM/dd/yyyy" />
            <script>
                var _formatDate = '@System.Configuration.ConfigurationManager.AppSettings["FormatDate"].ToString().ToUpper()';
            </script>
            <script src="@Url.Content("~/js/datepicker/daterangepicker.js")"></script>
        */
        //public static string _FormatDate = string.IsNullOrEmpty(ConfigurationManager.AppSettings["FormatDate"]) ? "dd/MM/yyyy" : ConfigurationManager.AppSettings["FormatDate"];
        public DynamicDisplayFormatAttribute()
        {
            DataFormatString = ConfigurationManager.AppSettings["FormatDate"].ToString();
        }
    }

}
