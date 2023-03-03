using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models
{
    public class OrderViewModel
    {
        public string OrderId { get; set; }
        public string ProductReference { get; set; }
        public string ProductName { get; set; }
        public int NumberOfItems { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string OrderStatus { get; set; }
        public int OrderProgress { get; set; }
        public string DateOrderCreated { get; set; }
        public string ExpectedDeliveryDate { get; set; }
        public decimal OrderWorth { get; set; }
        public decimal ShippingCost { get; set; }
    }
}