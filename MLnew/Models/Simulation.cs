using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MLnew.Models
{
    public class Simulation
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("AspNetUsers")]
        [Column("UserId")]
        public string UserId { get; set; }


        public int MethodeId { get; set; }
        public virtual Methode Methode { get; set; }

        public float Accuracy { get; set; }

        public string Duree { get; set; }//la duree peut etre convertie en string avec python

        public DateTime DateSimulation { get; set; }


    }
}
