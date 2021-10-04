using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PMSystem.Models
{
    public class UserTasks
    {
        [Key, Column(Order = 1)]
        public int UserId { get; set; }
        [Key, Column(Order = 2)]
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public User User { get; set; }
    }
}