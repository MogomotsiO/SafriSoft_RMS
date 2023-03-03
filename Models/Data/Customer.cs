using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string CustomerName  {get;set;}
        public string CustomerEmail {get;set;}
        public string CustomerCell  {get;set;}
        public string CustomerAddress {get;set;}
        public string DateCustomerCreated { get; set; }
        public int NumberOfOrders { get; set; }
        public string Status { get; set; }
    }
}