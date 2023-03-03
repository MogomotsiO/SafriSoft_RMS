using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public int TransactionCode { get; set; }
        public string TransactionName { get; set; }
        public decimal? TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public int TenantId { get; set; }
        public int TransactionLink { get; set; }

    }
}