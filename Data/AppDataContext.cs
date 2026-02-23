using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SecureWebAppDemo.Data
{
    public class AppDataContext
        : IdentityDbContext
    {


        public AppDataContext(DbContextOptions<AppDataContext> options)
            : base(options)
        {
              
        }
    }
}
