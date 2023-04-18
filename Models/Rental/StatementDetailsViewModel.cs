using SafriSoftv1._3.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class StatementDetailsViewModel
    {
        public Organisations organisation { get; set; }
        public Tenant tenant { get; set; }
        public Unit unit { get; set; }
        public List<Transaction> transactions { get; set; } = new List<Transaction>();
        public decimal? BalanceBF { get; set; }
        public decimal? Balance { get; set; }
    }
}