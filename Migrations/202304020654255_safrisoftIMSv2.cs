namespace SafriSoftv1._3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class safrisoftIMSv2 : DbMigration
    {
        public override void Up()
        {
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
            
            CreateTable(
                "dbo.Charge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Effective = c.DateTime(nullable: false),
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
                "dbo.UnitCharge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UnitId = c.Int(nullable: false),
                        FeeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Unit");
            DropTable("dbo.UnitCharge");
            DropTable("dbo.Transaction");
            DropTable("dbo.Tenant");
            DropTable("dbo.NOK");
            DropTable("dbo.InboxReplies");
            DropTable("dbo.InboxMessages");
            DropTable("dbo.Document");
            DropTable("dbo.Charge");
            DropTable("dbo.Assigned");
        }
    }
}
