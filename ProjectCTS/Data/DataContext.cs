using Microsoft.EntityFrameworkCore;

namespace ProjectCTS.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Register> registers => Set<Register>();
    }
}
