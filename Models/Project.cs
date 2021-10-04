using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PMSystem.Models
{
    public class Project
    {
        public Project()
        {
            AssignedUsers = new HashSet<User>();
            Tasks = new HashSet<Task>();
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
         [StringLength(255)]
        public string Description { get; set; }
        public float Status { get; set; }
        public User Owner { get; set; }
        public virtual ICollection<User> AssignedUsers { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}