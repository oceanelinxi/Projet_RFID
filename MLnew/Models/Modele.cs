using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace MLnew.Models
{
    public class Modele
    {
        [Key]
        public int ModeleID { get; set; }
        public int HistoriqueID { get; set; }
        public int DureeSec { get; set; }
        public string? Nom { get; set; }
        public float Accuracy {  get; set; }
        public virtual ICollection<Parametre> Parametres { get; set; } = new List<Parametre>();
        public virtual Historique Historique { get; set; }
    }
}
