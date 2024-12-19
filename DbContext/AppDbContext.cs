using Microsoft.EntityFrameworkCore;
using SFT.Model;

namespace SFT
{
    public class AppDbContext : DbContext
    {
        // DbSet for Users table
        public DbSet<User> Users { get; set; }
        public DbSet<UserAuth> UserAuthentication { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }




        // Constructor for AppDbContext
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
