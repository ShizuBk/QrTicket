using Microsoft.EntityFrameworkCore;
using TicketAPI.Entities;

namespace TicketAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Events> Events { get; set; }
        public DbSet<Fees> Fees { get; set; }
        public DbSet<TicketAssistants> TicketAssistants { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<Tokens> Tokens { get; set; }
        public DbSet<LocalConfig > LocalConfig { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<AuthLevel> AuthLevel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Events>().HasQueryFilter(e => e.SysEnabled);
        }

    }
}
