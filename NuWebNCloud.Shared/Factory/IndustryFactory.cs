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
    public class IndustryFactory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private BaseFactory _baseFactory = null;

        public IndustryFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<IndustryModels> GetData(List<String> listArea)
        {
            using (NuWebContext cxt = new NuWebContext())
            {
                var listData = (from tb in cxt.G_Industry
                                where tb.Status != 9 && tb.IsPublic && listArea.Contains(tb.ID/*tb.AreaName*/)
                                orderby tb.CreatedDate
                                select new IndustryModels
                                {
                                    ID = tb.ID,
                                    Name = tb.Name,
                                    AreaName = tb.AreaName,
                                }).ToList();
                return listData;
            }
        }
    }
}
