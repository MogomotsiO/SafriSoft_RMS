using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class NOK
    {
        [Key]
        public int Id { get; set; }
        public string NOKName { get; set; }
        public string NOKEmail { get; set; }
        public string NOKCell { get; set; }
        public string NOKRelation { get; set; }
        public DateTime DateNOKCreated { get; set; }
        public int TenantId { get; set; }
    }
}