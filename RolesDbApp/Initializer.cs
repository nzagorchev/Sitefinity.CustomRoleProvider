using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolesDbApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new RolesDbAppContext())
            {
                var role = new Role()
                {
                    Name = "MyRole",
                    Id = Guid.NewGuid(),
                };

                var role2 = new Role()
                {
                    Name = "MyRole2",
                    Id = Guid.NewGuid(),
                };

                db.Roles.Add(role);
                db.Roles.Add(role2);
                db.SaveChanges();
            }
        }
    }
}
