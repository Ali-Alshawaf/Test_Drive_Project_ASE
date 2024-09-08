using Microsoft.EntityFrameworkCore;

namespace Test_Drive.Models
{
    public class ODB : DbContext
    {
        public ODB(DbContextOptions<ODB> options) : base(options)
        {
        }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Cars> Cars { get; set; }
        public virtual DbSet<Book> Book { get; set; }
    }
}
