using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class Charge
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Code { get; set; }
        public int Type { get; set; }
        public DateTime Effective { get; set; }
        public int OrganisationId { get; set; }
    }
}