using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMSystem.Models
{
    public class User
    {
        public User()
        {
            AssignedProjects = new HashSet<Project>();
            AssignedSubTasks = new HashSet<SubTask>();
            AssignedTasks = new HashSet<Task>();
        }
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Vorname { get; set; }
        [Required, StringLength(100)]
        public string Nachname { get; set; }
        [Required]
        public string Passwort { get; set; }
        [Required]
        public int Level { get; set; } //with '0' for normal, '1' for Owner and '2' for admin
        [Required, StringLength(50),EmailAddress(ErrorMessage = "Die Email ist nicht valide!")]
        public string Email { get; set; }
        [StringLength(100), Display(Name ="Straße, Hausnummer")]
        public string Address { get; set; }
        //[StringLength(5)]
        public string PLZ { get; set; }
        [StringLength(50)]
        public string Bundesland { get; set; }
        [StringLength(50)]
        public string Land { get; set; }
        [StringLength(20)]
        public string Postfach { get; set; }
        [StringLength(20)]
        public string Telefonnummer { get; set; }
        [StringLength(20)]
        public string Faxnummer { get; set; }
        public virtual ICollection<Project> AssignedProjects { get; set; }
        public virtual ICollection<Task> AssignedTasks { get; set; }
        public virtual ICollection<SubTask> AssignedSubTasks { get; set; }
    }
}