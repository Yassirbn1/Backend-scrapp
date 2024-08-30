using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projetStage.Models
{
    [Table("WESM_users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Code { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [StringLength(100)]
        public string Departement { get; set; }

        public bool NeedsPasswordChange { get; set; }
        public bool IsActive { get; set; }

        public string Role { get; set; } // Ajout de la propriété Role

        // Role attributes
        public bool IsAdmin { get; set; }
        public bool IsPurchaser { get; set; }
        public bool IsRequester { get; set; }
        public bool IsValidator { get; set; }
        public bool ReOpenRequestAfterValidation { get; set; }

    }
}
