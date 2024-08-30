namespace scrapp_app.Models
{
    public class RegisterRequest
    {
        public int Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Departement { get; set; }
        public bool NeedsPasswordChange { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsPurchaser { get; set; }
        public bool IsRequester { get; set; }
        public bool IsValidator { get; set; }
        public bool ReOpenRequestAfterValidation { get; set; }
        public string Role { get; set; } // Ajout de la propriété Role
    }
}
