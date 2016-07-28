using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RolesDbApp
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        public virtual IEnumerable<User> Users { get; set; }

        public Role()
        {
            this.Users = new List<User>();
        }
    }
}
