using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class UnitViewModel
    {
        public int Id { get; set; }
        public string UnitNumber { get; set; }
        public string UnitName { get; set; }
        public int UnitRooms { get; set; }
        public string UnitSharing { get; set; }
        public string UnitDescription { get; set; }
        public decimal? UnitPrice { get; set; }
        public int NumberOfTenants { get; set; }
    }
}