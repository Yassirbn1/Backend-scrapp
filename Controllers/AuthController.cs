using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using scrapp_app.Data;
using scrapp_app.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using projetStage.Models;

namespace scrapp_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest.Code <= 0 || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest("Code and Password are required.");
            }

            var user = _context.Users.SingleOrDefault(u => u.Code == loginRequest.Code);

            if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid code or password." });
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Code.ToString()),
        new Claim(ClaimTypes.Name, user.FirstName),
        new Claim(ClaimTypes.GivenName, user.LastName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return Ok(new
            {
                Code = user.Code,
                FirstName = user.FirstName,  // Ajout du prénom
                LastName = user.LastName,    // Ajout du nom de famille
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }





        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest newUserRequest)
        {
            var existingUser = _context.Users.SingleOrDefault(u => u.Code == newUserRequest.Code);
            if (existingUser != null)
            {
                return BadRequest("User already exists.");
            }

            var newUser = new User
            {
                Code = newUserRequest.Code,
                FirstName = newUserRequest.FirstName,
                LastName = newUserRequest.LastName,
                Email = newUserRequest.Email,
                Password = newUserRequest.Password,
                Departement = newUserRequest.Departement,
                NeedsPasswordChange = newUserRequest.NeedsPasswordChange,
                IsActive = newUserRequest.IsActive,
                IsAdmin = newUserRequest.IsAdmin,
                IsPurchaser = newUserRequest.IsPurchaser,
                IsRequester = newUserRequest.IsRequester,
                IsValidator = newUserRequest.IsValidator,
                ReOpenRequestAfterValidation = newUserRequest.ReOpenRequestAfterValidation,
                Role = newUserRequest.Role // Ajoutez cette ligne pour définir le rôle
            };

            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("User registered successfully.");
        }


        [HttpGet("currentrole")]
        public IActionResult GetCurrentUserRole()
        {
            // Vérifier si l'utilisateur est authentifié
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Extraire le rôle de l'utilisateur à partir des revendications
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (roleClaim == null)
            {
                return NotFound("Role not found.");
            }

            return Ok(new { Role = roleClaim.Value });
        }

        [HttpGet("user")]
        public IActionResult GetUserDetails([FromQuery] int code)
        {
            var user = _context.Users.SingleOrDefault(u => u.Code == code);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new { FirstName = user.FirstName, LastName = user.LastName });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Implémentez ici la logique de déconnexion si nécessaire
            return Ok("Logout successful.");
        }

        private bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, hashedPassword);
        }

        private string GetUserRole(User user)
        {
            // Retourne le rôle de l'utilisateur en fonction de ses attributs
            if (user.IsAdmin) return "Admin";
            if (user.IsPurchaser) return "Purchaser";
            if (user.IsRequester) return "Requester";
            if (user.IsValidator) return "Validator";
            return "User";
        }
    }
}
