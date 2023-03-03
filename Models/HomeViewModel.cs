using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models
{
    public class HomeViewModel
    {
        public int CustomersTotal { get; set; }
        public int StockTotal { get; set; }
        public int ProductTotal { get; set; }
        public int OrdersProcessed { get; set; }
        public int OrdersPackaged { get; set; }
        public int OrdersInTransit { get; set; }
        public int OrdersDelivered { get; set; }

    }
}