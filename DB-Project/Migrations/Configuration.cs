namespace DB_Project.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DB_Project.DataBase.FileManager>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DB_Project.DataBase.FileManager context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            context.Permissions.AddOrUpdate(
              p => p.Name,
              new DataBase.Models.Permission { Name = "ReadOnly", ID = 1 },
              new DataBase.Models.Permission { Name = "Locked", ID = 2 },
              new DataBase.Models.Permission { Name = "Hidden", ID = 3 }
            );

            context.Roles.AddOrUpdate(new DataBase.Models.Role() { Name = "Admin" });

            context.Users.AddOrUpdate(new DataBase.Models.User() { Name = "Admin", Email = "Admin@Admin.com", Password = "Admin@123456", Role = context.Roles.SingleOrDefault(r => r.Name == "Admin") });
            //
        }
    }
}
