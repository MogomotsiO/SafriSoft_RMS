using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class OrderAudit
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Changed { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; }
        public string OrderId { get; set; }
    }
}