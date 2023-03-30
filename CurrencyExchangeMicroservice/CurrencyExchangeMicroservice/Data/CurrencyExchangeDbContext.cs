using CurrencyExchangeMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchangeMicroservice.Data
{
    // Define the CurrencyExchangeDbContext class, which inherits from DbContext.
    public class CurrencyExchangeDbContext : DbContext
    {
        // Constructor that takes DbContextOptions and passes them to the base class constructor.
        public CurrencyExchangeDbContext(DbContextOptions<CurrencyExchangeDbContext> options)
            : base(options)
        {
        }

        // DbSet property for accessing CurrencyExchangeTrade entities in the database.
        public DbSet<CurrencyExchangeTrade> CurrencyExchangeTrades { get; set; }
    }
}
