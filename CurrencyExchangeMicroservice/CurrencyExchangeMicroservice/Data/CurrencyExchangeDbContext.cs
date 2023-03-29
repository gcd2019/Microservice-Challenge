using CurrencyExchangeMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchangeMicroservice.Data
{
    public class CurrencyExchangeDbContext : DbContext
    {
        public CurrencyExchangeDbContext(DbContextOptions<CurrencyExchangeDbContext> options)
            : base(options)
        {
        }

        public DbSet<CurrencyExchangeTrade> CurrencyExchangeTrades { get; set; }
    }
}
