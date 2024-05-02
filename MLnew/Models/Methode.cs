﻿using System.ComponentModel.DataAnnotations;

namespace MLnew.Models
{
    public class Methode
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Nom { get; set; }

        public string Param1 { get; set; }

        public string Param2 { get; set; }

        public string Param3 { get; set; }
    }
}
