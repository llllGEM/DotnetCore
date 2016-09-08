using Microsoft.EntityFrameworkCore;

namespace ConsoleApplication.Data
{
    public class EFCoreContext : DbContext
    {
        public DbSet<Session> Sessions {get; set;}
        public DbSet<Vote> Votes {get; set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./EFCoreContext.db");
        }
    }
}