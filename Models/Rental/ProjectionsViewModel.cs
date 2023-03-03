using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class ProjectionsViewModel
    {
        public int Id { get; set; }
        public string TransDate { get; set; }
        public string TenantName { get; set; }
        public string TransName { get; set; }
        public decimal? TransProposed { get; set; }
        public decimal? TransActual { get; set; }
        public decimal? Balance { get; set; }

    }
}