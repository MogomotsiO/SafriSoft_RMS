using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class NOKViewModel
    {
        public int Id { get; set; }
        public string NOKName { get; set; }
        public string NOKEmail { get; set; }
        public string NOKCell { get; set; }
        public string NOKRelation { get; set; }
        public string DateNOKCreated { get; set; }
        public int TenantId { get; set; }
    }
}