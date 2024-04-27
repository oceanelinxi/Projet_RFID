using System.ComponentModel.DataAnnotations;

namespace ML1.Models
{
    public class Sauvegarde
    {
        [Key]
        public int IdSauvegarde { get; set; }

        public float Accuracy { get; set; }

        public DateTime Heure { get; set; }

        public int IdMethode { get; set; }

        public ICollection<Histo>? HistoHparam { get; set; }

    }
}
