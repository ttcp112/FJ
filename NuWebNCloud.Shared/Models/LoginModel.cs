using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuWebNCloud.Shared.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public List<string> ListStoreID { get; set; }
        public string StoreId { get; set; }
        public string StoreName { get; set; }

        public string IndustryId { get; set; }

        public string LanguageId { get; set; }
        public List<SelectListItem> ListLanguage { get; set; }
        public string UrlWebHost { get; set; }
        public string CountryCode { get; set; }
        public bool IsFormOtherLink { get; set; }
        public LoginModel() { ListLanguage = new List<SelectListItem>(); }

        public LoginModel(string returnUrl = null)
        {
            //ReturnUrl = (returnUrl == null) ? "Home/Index" : returnUrl;
            ReturnUrl = returnUrl;
            ListLanguage = new List<SelectListItem>();

        }
       
    }
}
