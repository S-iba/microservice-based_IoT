using Microsoft.EntityFrameworkCore;
using IoT.Shared.Models;

namespace IoT.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<ActionCommand> Commands { get; set; }
    }
}
