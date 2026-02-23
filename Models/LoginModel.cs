namespace SecureWebAppDemo.Models
{
    public record LoginModel(string email, string password, bool rememberMe, string? returnUrl);
    
}
