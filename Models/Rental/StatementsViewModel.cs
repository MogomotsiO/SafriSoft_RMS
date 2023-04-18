using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class StatementsViewModel
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public decimal? Balance { get; set; }
    }
}