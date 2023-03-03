using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class Assigned
    {
        [Key]
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int UnitId { get; set; }
        public int UnitRomms { get; set; }
    }
}