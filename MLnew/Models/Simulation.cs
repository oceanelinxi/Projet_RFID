using System.ComponentModel.DataAnnotations.Schema;

namespace MLnew.Models
{
    public class Simulation
    {
        public int Id { get; set; }
        [ForeignKey("AspNetUsers")]
        [Column("UserId")]
        public string UserId { get; set; }
        [ForeignKey("Methode")]
        [Column("MethodeId")]
        public int MethodeId { get; set; }

        public int Accuracy { get; set; }

        public string Duree { get; set; }//la duree peut etre convertie en string avec python

        public DateTime DateSimulation { get; set; }

    }
}
