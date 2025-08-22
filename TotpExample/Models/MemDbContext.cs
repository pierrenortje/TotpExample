using Microsoft.EntityFrameworkCore;

namespace TotpExample.Models
{
    public class MemDbContext : DbContext
    {
        public MemDbContext(DbContextOptions<MemDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new User { Id = 1, Email = "john.doe@example.com", Secret = null } );
        }
    }
}
