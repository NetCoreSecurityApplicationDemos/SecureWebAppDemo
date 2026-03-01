using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SecureWebAppDemo.ViewModel
{
    public class Verify2FAModel
    {
        [Required]
        [Display(Name = "Doğrulama Kodu")]
        public string Code { get; set; }
        public string? ReturnUrl { get; set; }

        public bool RememberMe { get; set; }
    }
}
