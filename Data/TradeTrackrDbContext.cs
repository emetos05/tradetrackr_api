using Microsoft.EntityFrameworkCore;
using tradetrackr.api.Models;

namespace tradetrackr.api.Data
{
    public class TradeTrackrDbContext(DbContextOptions<TradeTrackrDbContext> options) : DbContext(options)
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Job> Jobs { get; set; }
    }    
    
}
