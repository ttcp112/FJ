﻿using System.ComponentModel.DataAnnotations;

namespace NuWebNCloud.Shared.Integration.Models.Sandbox.Inventory.Product
{
    public class InteProductOnGroupModels
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Please enter a value greater than or equal to 0")]
        public double ExtraPrice { get; set; }
        public int Sequence { get; set; }

        public int Seq { get; set; }
        public int OffSet { get; set; }
        public string Name { get; set; }
        public string GroupID { get; set; }
        public byte Status { get; set; }
        public bool Checked { get; set; }
    }
}