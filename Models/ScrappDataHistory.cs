using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projetStage.Models
{
    [Table("ScrappDataHistory")]
    public class ScrappDataHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Identifiant unique pour chaque modification

        [Required]
        public DateTime DateTime { get; set; } // Date et heure de la modification

        [Required]
        [ForeignKey("UserCode")]
        public User User { get; set; } // Utilisateur ayant effectué la modification

        public int UserCode { get; set; } // Code de l'utilisateur, clé étrangère

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } // Type d'action (Insert, Update, Delete)

        [Required]
        [StringLength(50)]
        public string TableType { get; set; } // Type de table (ScrappData ou ScrappDataShift)
    }
}
