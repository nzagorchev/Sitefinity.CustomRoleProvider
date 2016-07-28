using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RolesDbApp
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "DisplayName is required.")]
        public string DisplayName { get; set; } 

        public virtual IEnumerable<Role> Roles { get; set; }

        public User()
        {
            this.Roles = new List<Role>();
        }
    }
}
