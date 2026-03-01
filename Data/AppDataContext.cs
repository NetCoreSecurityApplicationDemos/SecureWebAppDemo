using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureWebAppDemo.Models;

namespace SecureWebAppDemo.Data
{
    public class AppDataContext
        : IdentityDbContext
    {

        public DbSet<AppUser> AppUsers { get; set; }    
        public AppDataContext(DbContextOptions<AppDataContext> options)
            : base(options)
        {
              
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUser");
            });
        }
    }
}
