using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class AssignedTenantViewModel
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
        public string TenantEmail { get; set; }
        public string TenantCell { get; set; }
        public string Assigned { get; set; }
        public int UnitId { get; set; }
    }
}