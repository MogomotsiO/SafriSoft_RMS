using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class TransactionReportsViewModel
    {
        public string TransactionCode { get; set; }
        public string TransactionName { get; set; }
        public string TransactionDate { get; set; }
        public string Tenant { get; set; }
        public decimal TransactionAmount { get; set; }
    }
}