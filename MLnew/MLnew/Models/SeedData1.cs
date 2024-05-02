using Microsoft.EntityFrameworkCore;
using MLnew.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using MLnew.Models;
namespace MLnew.Models
{
    public class SeedData1
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<
            DbContextOptions<ApplicationDbContext>>()))
            {
                context.Database.EnsureCreated();
                // S’il y a déjà des films dans la base
                if (context.Simulation.Any())
                {
                    return; // On ne fait rien
                }
                // Sinon on en ajoute un
                context.Simulation.AddRange(
                new Simulation
                {
                    UserId = "3cc83b57-f298-4cec-8d15-e75ee9fd1d4f",
                    MethodeId = 1,
                    Accuracy = 1,
                    Duree = "2min",
                    DateSimulation = DateTime.Now,
                }
                );
                context.SaveChanges();
            }
        }
    }
}
