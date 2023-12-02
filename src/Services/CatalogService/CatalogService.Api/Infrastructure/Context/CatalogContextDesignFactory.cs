using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
                .UseSqlServer("Data Source=(localdb)\\localhost;Initial Catalog=catalog;Persist Security Info=True;User ID=sa;Password=Onur123!");

            return new CatalogContext(optionsBuilder.Options);
        }
    }
}
