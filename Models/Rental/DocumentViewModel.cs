using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Rental
{
    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string DateFileCreated { get; set; }
        public int TenantId { get; set; }
    }
}