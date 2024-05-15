using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Simulation>()
                .HasOne(e => e.Methode)
                .WithMany(e => e.Simulations)
                .HasForeignKey(e => e.MethodeId)
                .IsRequired();
           
        }
    }
}
