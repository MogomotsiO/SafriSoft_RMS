using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductReference { get; set; }
        public string ProductCode { get; set; }
        public string ProductCategory { get; set; }
        public string ProductImage { get; set; }
        public double SellingPrice { get; set; }
        public int ItemsSold { get; set; }
        public int ItemsAvailable { get; set; }
    }
}