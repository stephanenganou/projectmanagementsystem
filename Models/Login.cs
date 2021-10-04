using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMSystem.Models
{
    public class Login
    {
        [Required]
        [EmailAddress(ErrorMessage = "Die Email ist nicht valide!")]
        public string Username { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Passwort nicht lang genau (min 8 Zeichen)")]
        public string Password { get; set; }
    }
}