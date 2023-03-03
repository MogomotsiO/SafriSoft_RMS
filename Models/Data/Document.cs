using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; }
        public byte[] FileByte { get; set; }
        public string FileContentType { get; set; }
        public DateTime DateFileCreated { get; set; }
        public int TenantId { get; set; }
    }
}