using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class ChargeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Code { get; set; }
        public int Type { get; set; }
        public string Effective { get; set; }
        public int OrganisationId { get; set; }
        public int UnitId { get; set; }
        public bool UnitAssigned { get; set; }
        public int UnitChargeId { get; set; }
    }
}