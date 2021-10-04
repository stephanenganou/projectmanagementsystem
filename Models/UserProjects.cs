using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PMSystem.Models
{
    public class UserProjects
    {
        [Key, Column(Order = 1)]
        public int UserId { get; set; }
        [Key, Column(Order = 2)]
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public User User { get; set; }
    }
}