using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class Tenant
    {
        [Key]
        public int Id { get; set; }
        public string TenantName { get; set; }
        public string TenantEmail { get; set; }
        public string TenantCell { get; set; }
        public string TenantAddress { get; set; }
        public string TenantWorkAddress { get; set; }
        public string TenantWorkCell { get; set; }
        public DateTime DateTenantCreated { get; set; }
        public DateTime DateLeaseStart { get; set; }
        public DateTime DateLeaseEnd { get; set; }
        public string Status { get; set; }
    }
}