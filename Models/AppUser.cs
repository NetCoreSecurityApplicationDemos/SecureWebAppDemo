using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureWebAppDemo.Models
{
     
    public class AppUser
        : IdentityUser
    {
        public bool Enabled { get; set; }   
        public bool IsMfaEnabled { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
