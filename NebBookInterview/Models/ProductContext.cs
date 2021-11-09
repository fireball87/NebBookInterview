using Microsoft.EntityFrameworkCore;

namespace NebBookInterview.Models
{
    public class ProductContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductPriceChange> ProductPriceChanges => Set<ProductPriceChange>();
        
        // Username and password are set by PGUSER and PGPASSWORD environment variables
        // I use snake case because that is preferred convention for postgres databases.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseNpgsql("Host=localhost;Database=neb_book_interview_dev;")
                .UseSnakeCaseNamingConvention();
        

    }
}