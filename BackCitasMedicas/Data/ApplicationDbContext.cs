using BackCitasMedicas.Models;
using Microsoft.EntityFrameworkCore;

namespace BackCitasMedicas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
