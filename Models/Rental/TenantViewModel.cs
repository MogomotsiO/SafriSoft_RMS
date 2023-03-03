using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class TenantViewModel
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
        public string TenantEmail { get; set; }
        public string TenantCell { get; set; }
        public string TenantAddress { get; set; }
        public string TenantWorkAddress { get; set; }
        public string TenantWorkCell { get; set; }
        public string DateTenantCreated { get; set; }
        public int Documents { get; set; }
        public int NOK { get; set; }
        public string DateLeaseStart { get; set; }
        public string DateLeaseEnd { get; set; }

    }
}