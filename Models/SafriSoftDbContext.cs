using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using SafriSoftv1._3.Models.Data;

namespace SafriSoftv1._3.Models
{
    public class SafriSoftDbContext : DbContext
    {
        public SafriSoftDbContext() : base("name=SafriSoftDbContext")
        {
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<InboxMessages> InboxMessages { get; set; }
        public virtual DbSet<InboxReplies> InboxReplies { get; set; }
        public virtual DbSet<OrderAudit> OrderAudit { get; set; }
        public virtual DbSet<Organisations> Organisations { get; set; }
        public virtual DbSet<Tenant> Tenants { get; set; }
        public virtual DbSet<NOK> NOKs { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<Assigned> Assigned { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public static SafriSoftDbContext Create()
        {
            return new SafriSoftDbContext();
        }
    }
}