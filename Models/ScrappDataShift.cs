using projetStage.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace scrapp_app.Models
{
    public class ScrappDataShift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public int Purge { get; set; }
        public int DefautInjection { get; set; }
        public int DefautAssemblage { get; set; }
        public int Bavures { get; set; }
        public int Shift { get; set; }

        // Nouvelle propriété ajoutée
        [Required]
        public int Code { get; set; } // Modifier ici pour utiliser Code au lieu de Matricule

        // Propriété de navigation
        [ForeignKey("Code")]
        public User User { get; set; }
   
    }
}
