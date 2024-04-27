using System.ComponentModel.DataAnnotations;

namespace ML1.Models
{
    public class Methode
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Nom { get; set; }

        public int? NbrHParam { get; set; }

        public ICollection<Hyperparametre>? Hyperparams { get; set; }
        public ICollection<Sauvegarde>? Sauvegardes { get; set; }
    }
}
