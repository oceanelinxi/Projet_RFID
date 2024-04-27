using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ML1.Models
{
    public class Histo
    {
        [Key]
        public int IdHisto { get; set; }

        [ForeignKey("Sauvegarde")]
        public int IdSauvegarde { get; set; }

        [ForeignKey("Hyperparamètres")]
        public int IdHParam { get; set; }

        [Required]
        [StringLength(255)]
        public string SelectedValue { get; set; }

        public virtual Sauvegarde Sauvegarde { get; set; }

        public virtual Hyperparametre Hyperparametre { get; set; }
    }
}
