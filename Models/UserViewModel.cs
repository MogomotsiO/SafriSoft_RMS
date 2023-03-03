using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public int NumberOfOrders { get; set; }
        public decimal RandValueSold { get; set; }
        public string UserState { get; set; }
    }
}