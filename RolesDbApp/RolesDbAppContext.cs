using System.Data.Entity;

namespace RolesDbApp
{
    public class RolesDbAppContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        public RolesDbAppContext()
            : base("RolesDbApp")
        {
        }
    }
}
