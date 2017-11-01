namespace DB_Project.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserRoles", "RoleID", "dbo.Roles");
            DropForeignKey("dbo.UserRoles", "UserID", "dbo.Users");
            DropIndex("dbo.UserRoles", new[] { "UserID" });
            DropIndex("dbo.UserRoles", new[] { "RoleID" });
            CreateTable(
                "dbo.RoleUsers",
                c => new
                    {
                        Role_ID = c.Int(nullable: false),
                        User_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Role_ID, t.User_ID })
                .ForeignKey("dbo.Roles", t => t.Role_ID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_ID, cascadeDelete: true)
                .Index(t => t.Role_ID)
                .Index(t => t.User_ID);
            
            DropTable("dbo.UserRoles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        RoleID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserID, t.RoleID });
            
            DropForeignKey("dbo.RoleUsers", "User_ID", "dbo.Users");
            DropForeignKey("dbo.RoleUsers", "Role_ID", "dbo.Roles");
            DropIndex("dbo.RoleUsers", new[] { "User_ID" });
            DropIndex("dbo.RoleUsers", new[] { "Role_ID" });
            DropTable("dbo.RoleUsers");
            CreateIndex("dbo.UserRoles", "RoleID");
            CreateIndex("dbo.UserRoles", "UserID");
            AddForeignKey("dbo.UserRoles", "UserID", "dbo.Users", "ID", cascadeDelete: true);
            AddForeignKey("dbo.UserRoles", "RoleID", "dbo.Roles", "ID", cascadeDelete: true);
        }
    }
}
