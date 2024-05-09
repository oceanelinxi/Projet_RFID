using Microsoft.EntityFrameworkCore;
using MLnew.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
namespace MLnew.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<
            DbContextOptions<ApplicationDbContext>>()))
            {
                context.Database.EnsureCreated();
                // S’il y a déjà des films dans la base
                if (context.Methode.Any())
                {
                    return; // On ne fait rien
                }
                // Sinon on en ajoute un
                context.Methode.AddRange(
                
                );
                context.SaveChanges();
            }
        }
    }
}

