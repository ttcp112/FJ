﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Models.Sandbox.Import
{
    public class SetMenuImportResultItem
    {
        public string Name { get; set; }
        public List<string> ListFailStoreName { get; set; }
        public List<string> ListSuccessStoreName { get; set; }
        //public string StoreNames { get; set; }
        //public int Count { get; set; }
        public List<SetMenuErrorItem> ErrorItems { get; set; }

        public SetMenuImportResultItem()
        {
            ListFailStoreName = new List<string>();
            ListSuccessStoreName = new List<string>();
            ErrorItems = new List<SetMenuErrorItem>();
        }
    }

    public class SetMenuErrorItem
    {
        public string GroupName { get; set; }
        public string ErrorMessage { get; set; }
    }
}
