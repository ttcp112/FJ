using Newtonsoft.Json;
using NLog;
using NuWebNCloud.Data;
using NuWebNCloud.Data.Entities;
using NuWebNCloud.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuWebNCloud.Shared.Factory
{
    public class OrderPaidFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public OrderPaidFactory()
        {
            _baseFactory = new BaseFactory();
        }

        //public bool GetDetailTaxForStore(string StoreId)
        //{
        //    using (NuWebContext cxt = new NuWebContext())
        //    {
        //        var tax = (from tb in cxt.G_Tax
        //                   where tb.IsActive == true && tb.StoreId == StoreId
        //                   orderby tb.DateCreated
        //                   select tb.IsTaxItemPrice).FirstOrDefault();
        //        return tax;
        //    }
        //}
    }
}
