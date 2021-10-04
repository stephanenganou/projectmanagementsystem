using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMSystem.Models
{
    public class Task
    {
        public Task()
        {
            SubTasks = new HashSet<SubTask>();
            AssignedUsers = new HashSet<User>();
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
        public float Status { get; set; }
        public Project Project { get; set; }
        public virtual ICollection<SubTask> SubTasks { get; set; }
        public virtual ICollection<User> AssignedUsers { get; set; }
    }
}