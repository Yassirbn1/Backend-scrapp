using projetStage.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace scrapp_app.Models
{
    public class ScrappData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public int QuantitéRetour { get; set; }
        public int QuantitéRestantePr { get; set; }
        

        // Propriété de navigation
        [ForeignKey("Code")]
        public User User { get; set; }

        public int Code { get; set; }
    }
}
