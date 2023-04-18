using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SafriSoftv1._3.Models.Data
{
    public class OrganisationSoftware
    {
        [Key]
        public int Id { get; set; }
        public int OrganisationId { get; set; }
        public int SoftwareId { get; set; }
        public int PackageId { get; set; }
        public bool Granted { get; set; }
    }
}