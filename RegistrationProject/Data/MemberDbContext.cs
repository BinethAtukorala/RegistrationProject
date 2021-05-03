using Microsoft.EntityFrameworkCore;
using RegistrationProject.Models;

namespace RegistrationProject.Data
{
    public class MemberDbContext : DbContext
    {
        private readonly string connString;
        public MemberDbContext(string connectionString)
        {
            connString = connectionString;
        }
        public DbSet<Member> Members { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connString);
        }
    }
}
