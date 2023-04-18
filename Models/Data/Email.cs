using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class Email
    {
        [Key] 
        public int Id { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string CcAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string EmailStatus { get; set; }
    }
}