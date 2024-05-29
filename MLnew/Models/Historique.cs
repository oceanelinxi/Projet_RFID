using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLnew.Models
{
    public class Historique
    {
       
        public int HistoriqueID { get; set; }
        [ForeignKey("AspNetUsers")]
        [Column("UserId")]
        public string UserId { get; set; }

        public virtual IdentityUser User { get; set; }
        public DateTime DateSimulation { get; set; }
        public virtual ICollection<Modele> Modeles { get; set; } = new List<Modele>();
    }
}
