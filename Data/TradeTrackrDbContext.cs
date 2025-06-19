using Microsoft.EntityFrameworkCore;
using tradetrackr.api.Models;

namespace tradetrackr.api.Data
{
    public class TradeTrackrDbContext(DbContextOptions<TradeTrackrDbContext> options) : DbContext(options)
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Job> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Jobs)
                .WithOne(j => j.Client)
                .HasForeignKey(j => j.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Invoices)
                .WithOne(i => i.Client)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Job>()
                .HasMany(j => j.Invoices)
                .WithOne(i => i.Job)
                .HasForeignKey(i => i.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }    
}
