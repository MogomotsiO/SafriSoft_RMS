namespace SafriSoftv1._3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class safriapplication : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            DropTable("dbo.Assigned");
            DropTable("dbo.Customer");
            DropTable("dbo.Document");
            DropTable("dbo.InboxMessages");
            DropTable("dbo.InboxReplies");
            DropTable("dbo.NOK");
            DropTable("dbo.OrderAudit");
            DropTable("dbo.Orders");
            DropTable("dbo.Organisations");
            DropTable("dbo.Product");
            DropTable("dbo.Tenant");
            DropTable("dbo.Transaction");
            DropTable("dbo.Unit");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Unit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UnitNumber = c.String(),
                        UnitName = c.String(),
                        UnitRooms = c.Int(nullable: false),
                        Sharing = c.String(),
                        UnitDescription = c.String(),
                        UnitPrice = c.Decimal(precision: 18, scale: 2),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Transaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionCode = c.Int(nullable: false),
                        TransactionName = c.String(),
                        TransactionAmount = c.Decimal(precision: 18, scale: 2),
                        TransactionDate = c.DateTime(nullable: false),
                        TenantId = c.Int(nullable: false),
                        TransactionLink = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tenant",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantName = c.String(),
                        TenantEmail = c.String(),
                        TenantCell = c.String(),
                        TenantAddress = c.String(),
                        TenantWorkAddress = c.String(),
                        TenantWorkCell = c.String(),
                        DateTenantCreated = c.DateTime(nullable: false),
                        DateLeaseStart = c.DateTime(nullable: false),
                        DateLeaseEnd = c.DateTime(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Product",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductName = c.String(),
                        ProductReference = c.String(),
                        ProductCode = c.String(),
                        ProductCategory = c.String(),
                        ProductImage = c.String(),
                        SellingPrice = c.Double(nullable: false),
                        ItemsSold = c.Int(),
                        ItemsAvailable = c.Int(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Organisations",
                c => new
                    {
                        OrganisationId = c.Int(nullable: false, identity: true),
                        OrganisationName = c.String(),
                        OrganisationEmail = c.String(),
                        OrganisationCell = c.String(),
                        OrganisationLogo = c.String(),
                        OrganisationStreet = c.String(),
                        OrganisationSuburb = c.String(),
                        OrganisationCity = c.String(),
                        OrganisationCode = c.Int(nullable: false),
                        AccountName = c.String(),
                        AccountNo = c.Int(nullable: false),
                        BankName = c.String(),
                        BranchName = c.String(),
                        BranchCode = c.String(),
                        ClientReference = c.String(),
                        VATNumber = c.Int(nullable: false),
                        ImgLogoSource = c.String(),
                    })
                .PrimaryKey(t => t.OrganisationId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.String(),
                        ProductName = c.String(),
                        NumberOfItems = c.Int(nullable: false),
                        CustomerId = c.Int(nullable: false),
                        CustomerName = c.String(),
                        OrderStatus = c.String(),
                        OrderProgress = c.Int(nullable: false),
                        DateOrderCreated = c.String(),
                        ExpectedDeliveryDate = c.String(),
                        Status = c.String(),
                        OrderWorth = c.Decimal(precision: 18, scale: 2),
                        ShippingCost = c.Decimal(precision: 18, scale: 2),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OrderAudit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        Changed = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        UserId = c.String(),
                        OrderId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NOK",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NOKName = c.String(),
                        NOKEmail = c.String(),
                        NOKCell = c.String(),
                        NOKRelation = c.String(),
                        DateNOKCreated = c.DateTime(nullable: false),
                        TenantId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InboxReplies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Subject = c.String(),
                        Body = c.String(),
                        From = c.String(),
                        To = c.String(),
                        Status = c.String(),
                        MessageId = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InboxMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Body = c.String(),
                        From = c.String(),
                        To = c.String(),
                        Status = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Document",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        FileByte = c.Binary(),
                        FileContentType = c.String(),
                        DateFileCreated = c.DateTime(nullable: false),
                        TenantId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Customer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerName = c.String(),
                        CustomerEmail = c.String(),
                        CustomerCell = c.String(),
                        CustomerAddress = c.String(),
                        DateCustomerCreated = c.String(),
                        NumberOfOrders = c.Int(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Assigned",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        UnitId = c.Int(nullable: false),
                        UnitRomms = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
        }
    }
}
