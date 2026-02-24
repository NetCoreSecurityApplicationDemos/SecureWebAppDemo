using System.ComponentModel.DataAnnotations;

namespace SecureWebAppDemo.Models
{
    public record LoginModel(string email,
                            [Required]
                            [StringLength(4)]
                            [DataType(DataType.Password)]
                            string password,
                            bool rememberMe, string? returnUrl);
    
}
