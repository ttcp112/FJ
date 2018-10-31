using ClosedXML.Excel;
using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace NuWebNCloud.Shared.Factory
{
    public class DiscountDetailsReportDetailFactory : ReportFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;
        public DiscountDetailsReportDetailFactory()
        {
            _baseFactory = new BaseFactory();
        }

      

       
    }
}
