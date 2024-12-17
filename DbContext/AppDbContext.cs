using Microsoft.EntityFrameworkCore;
using SFT.Model;

namespace SFT
{
    public class AppDbContext : DbContext
    {
        // DbSet for Users table
        public DbSet<User> Users { get; set; }

        // Constructor for AppDbContext
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
