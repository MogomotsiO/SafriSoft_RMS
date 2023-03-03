using SafriSoftv1._3.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models
{
    public class InboxViewModel
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Status { get; set; }
        public string DateCreated { get; set; }
        public string MainUser { get; set; }
        public List<InboxReplies> Replies { get; set; } 
    }
}