namespace PublicOrders.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Login = c.String(nullable: false, maxLength: 120, unicode: false),
                        Password = c.String(nullable: false, maxLength: 120, unicode: false),
                        UserStatusId = c.Int(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.UserStatus", t => t.UserStatusId)
                .Index(t => t.Login)
                .Index(t => t.Password)
                .Index(t => t.UserStatusId);
            
            CreateTable(
                "dbo.UserStatus",
                c => new
                    {
                        UserStatusId = c.Int(nullable: false, identity: true),
                        StatusName = c.String(nullable: false, maxLength: 50, unicode: false),
                    })
                .PrimaryKey(t => t.UserStatusId)
                .Index(t => t.StatusName);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "UserStatusId", "dbo.UserStatus");
            DropIndex("dbo.UserStatus", new[] { "StatusName" });
            DropIndex("dbo.Users", new[] { "UserStatusId" });
            DropIndex("dbo.Users", new[] { "Password" });
            DropIndex("dbo.Users", new[] { "Login" });
            DropTable("dbo.UserStatus");
            DropTable("dbo.Users");
        }
    }
}
