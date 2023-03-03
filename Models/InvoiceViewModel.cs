using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models
{
    public class InvoiceViewModel
    {
        // Organisation Details
        public string OrganisationName { get; set; }
        public string OrganisationEmail { get; set; }
        public string OrganisationCell { get; set; }
        public string OrganisationLogo { get; set; }
        public string OrganisationStreet { get; set; }
        public string OrganisationSuburb { get; set; }
        public string OrganisationCity { get; set; }
        public int OrganisationCode { get; set; }
        public string AccountName { get; set; }
        public int AccountNo { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string ClientReference { get; set; }
        public int VATNumber { get; set; }
        public string ImgLogoSource { get; set; }

        // Customer Details
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerCell { get; set; }
        public string CustomerAddress { get; set; }

        // Order Details
        public string OrderId { get; set; }
        public string ProductName { get; set; }
        public int NumberOfItems { get; set; }
        public decimal OrderWorth { get; set; }
        public decimal ShippingCost { get; set; }
        public string DateOrderCreated { get; set; }

        // Invoice Totals
        public decimal VatAmount { get; set; }
        public decimal InvoiceTotal { get; set; }

    }
}