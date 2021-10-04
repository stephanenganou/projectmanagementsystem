using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMSystem.Models
{
    public class SubTask
    {
        public SubTask()
        {
            //empty constructor
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
        public float Status { get; set; }
        public int Duration { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Task Task { get; set; }
        public User User { get; set; }

    }
}