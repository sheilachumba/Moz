using Microsoft.EntityFrameworkCore;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Context
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Users> Users { get; set; }
    }
}
