using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models
{
    public class OrderAuditViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Changed { get; set; }
        public string CreatedDate { get; set; }
        public string UserId { get; set; }
        public int OrderId { get; set; }
    }
}