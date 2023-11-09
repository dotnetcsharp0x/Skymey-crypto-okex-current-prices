using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using Skymey_main_lib.Models.Prices;
using Skymey_main_lib.Models.Prices.Okex;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Skymey_crypto_okex_current_prices.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<OkexCurrentPrices> OkexCurrentPricesView { get; init; }
        public DbSet<CurrentPrices> CurrentPrices { get; init; }
        public static ApplicationContext Create(IMongoDatabase database) =>
            new(new DbContextOptionsBuilder<ApplicationContext>()
                .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
                .Options);
        public ApplicationContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<OkexCurrentPrices>().ToCollection("crypto_current_okex_prices");
            modelBuilder.Entity<CurrentPrices>().ToCollection("crypto_current_prices");
        }
    }
}
