using Mircosoft.EntityFrameworkCore;

namespace eCommerce.DataAccessLayer.DbContext;

public class ApplicationDbcontaxt : DbContext
{
    public ApplicationDbcontaxt(DbContextOptions<ApplicationDbcontaxt> options): base(options)
    {
        
    }

    public DbSet<Product> Products { get; set; }

    protected override void onModelCreating (ModelBuilder modelBuilder)
    {
        base.onModelCreating(modelBuilder);
    }
}
