using System.ComponentModel.DataAnnotations;

namespace ML1.Models
{
    public class Hyperparametre
    {
        [Key]
        public int IdHParam { get; set; }
        [Required]
        public string? NomHParam { get; set; }
        public string? Type { get; set; }
        public string? DefaultValue { get; set; }
        public ICollection<Histo>? HistoHparam { get; set; }
    }
}
