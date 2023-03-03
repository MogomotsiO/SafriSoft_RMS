using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class InboxReplies
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Status { get; set; }
        public int MessageId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}