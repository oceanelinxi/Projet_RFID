using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLnew.Models
{
    public class Parametre
    {
        
        public int ParametreID { get; set; }
        public int ModeleID { get; set; }
        public string? Nom { get; set; }
        public string? Valeur { get; set; }
    }
}
