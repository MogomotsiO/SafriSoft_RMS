using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }
        public string UnitNumber { get; set; }
        public string UnitName { get; set; }
        public int UnitRooms { get; set; }
        public string Sharing { get; set; }
        public string UnitDescription { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Status { get; set; }
    }
}