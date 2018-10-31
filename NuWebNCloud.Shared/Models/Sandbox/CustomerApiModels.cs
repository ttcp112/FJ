using NuWebNCloud.Shared.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox
{
    public class CustomerApiModels
    {
        public string StoreID { get; set; }
        public CustomerModels CustomerDTO { get; set; }

        public string ID { get; set; }
        public string IC { get; set; }
        public string Name { get; set; }
        public string ImageURL { get; set; }
        public string ImageData { get; set; }
        public bool IsActive { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool Gender { get; set; }
        public bool Marital { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime Anniversary { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsMembership { get; set; }
        public string HomeStreet { get; set; }
        public string HomeCity { get; set; }
        public string HomeZipCode { get; set; }
        public string HomeCountry { get; set; }
        public string OfficeStreet { get; set; }
        public string OfficeCity { get; set; }
        public string OfficeZipCode { get; set; }
        public string OfficeCountry { get; set; }
        public double TotalPaidAmout { get; set; }
        public double ByCash { get; set; }
        public double ByExTerminal { get; set; }
        public double ByGiftCard { get; set; }
        public double TotalRefund { get; set; }
        public DateTime LastVisited { get; set; }
        public int Reservation { get; set; }
        public int Cancelation { get; set; }
        public int WalkIn { get; set; }
        public List<ReceiptHistory> ListReceiptHistories { get; set; }

        public List<string> ListStoreID { get; set; }
        //=============
        public DateTime CreatedDate { get; set; }
        public string CreatedUser { get; set; }
        public string ModifiedUser { get; set; }
        public DateTime LastModified { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public RegisterTokenModels RegisterToken { get; set; }
        public byte Mode { get; set; }

        public List<string> ListOrgID { get; set; }
    }
}
