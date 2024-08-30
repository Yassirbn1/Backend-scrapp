

namespace scrapp_app.Models
{
    public class Register
    {
        public int Matricule { get; set; }
        public string Password { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Role { get; set; } // Nouvelle propriété pour le rôle
    }
}


