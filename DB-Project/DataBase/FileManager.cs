namespace DB_Project.DataBase
{
    using DB_Project.DataBase.Models;
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class FileManager : DbContext
    {
        // Your context has been configured to use a 'FileManager' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'DB_Project.DataBase.FileManager' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'FileManager' 
        // connection string in the application configuration file.
        public FileManager()
            : base("name=FileManagerConnection")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

         public virtual DbSet<User> Users { get; set; }
         public virtual DbSet<Role> Roles { get; set; }
         public virtual DbSet<Content> Contents { get; set; }
         public virtual DbSet<Permission> Permissions { get; set; }
         public virtual DbSet<UserContentPermission> UserContentPermissions { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}