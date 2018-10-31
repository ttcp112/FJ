using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Api
{
    public class CountryApiModels
    {
        public string Name { get; set; }
        public List<string> CallingCodes { get; set; }
        public string Region { get; set; }
        public List<string> Timezones { get; set; }
        public string Alpha2Code { get; set; }
        public string NumericCode { get; set; }
        public List<CurrencyCountryApiModels> Currencies { get; set; }
        public CountryApiModels()
        {
            CallingCodes = new List<string>();
            Timezones = new List<string>();
            Currencies = new List<CurrencyCountryApiModels>();
        }
    }

    public class CurrencyCountryApiModels
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}
