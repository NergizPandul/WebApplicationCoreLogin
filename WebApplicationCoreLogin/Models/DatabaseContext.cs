using Microsoft.EntityFrameworkCore;

namespace WebApplicationCoreLogin.Models
{
    public class DatabaseContext:DbContext
    {

        public DatabaseContext(DbContextOptions options):base(options)
        {

        }
        public virtual DbSet<User> Users { get; set; }

    }
}
