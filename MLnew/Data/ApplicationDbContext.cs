using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MLnew.Models;

namespace MLnew.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<MLnew.Models.Methode>? Methode { get; set; }
        public DbSet<MLnew.Models.Simulation>? Simulation { get; set; }
    }
}
