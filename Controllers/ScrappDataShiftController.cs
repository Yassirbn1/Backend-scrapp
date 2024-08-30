using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using projetStage.Models;
using scrapp_app.Data;
using scrapp_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace scrapp_app.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScrappDataShiftController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ScrappDataShiftController> _logger;

        public ScrappDataShiftController(AppDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<ScrappDataShiftController> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private int GetCurrentUserMatricule()
        {
            var userClaims = _httpContextAccessor.HttpContext.User.Claims;
            foreach (var claim in userClaims)
            {
                _logger.LogInformation($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }

            var matriculeClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (matriculeClaim != null && int.TryParse(matriculeClaim.Value, out int matricule))
            {
                _logger.LogInformation($"Current user matricule: {matricule}");
                return matricule;
            }
            _logger.LogWarning("Matricule utilisateur non trouvé.");
            throw new UnauthorizedAccessException("Matricule utilisateur non trouvé.");
        }

        [HttpGet]
        public IActionResult GetScrappDataShifts()
        {
            try
            {
                // Récupérez la liste des données
                var scrappDataShiftList = _context.ScrappDataShift.ToList();

                // Ajoutez un log pour vérifier le contenu de scrappDataShiftList
                if (scrappDataShiftList == null || !scrappDataShiftList.Any())
                {
                    _logger.LogInformation("No data found in ScrappDataShift.");
                    // Retournez une liste vide avec le code 200 OK
                    return Ok(new List<ScrappDataShift>());
                }

                _logger.LogInformation("ScrappDataShift retrieved successfully.");
                return Ok(scrappDataShiftList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving ScrappDataShift.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




        [HttpGet("shift/{shiftValue}")]
        public IActionResult GetScrappDataShiftByShiftValue(int shiftValue)
        {
            try
            {
                var scrappDataShiftList = _context.ScrappDataShift
                    .Include(s => s.User)
                    .Where(d => d.Shift == shiftValue)
                    .ToList();

                _logger.LogInformation($"ScrappDataShift for shift {shiftValue} retrieved.");
                return Ok(scrappDataShiftList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving ScrappDataShift by shift value.");
                return StatusCode(500, $"Erreur interne du serveur : {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostScrappDataShift([FromBody] ScrappDataShift scrappDataShift)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid ScrappDataShift model state.");
                return BadRequest(ModelState);
            }

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return Unauthorized();
            }

            var currentMatricule = GetCurrentUserMatricule();
            scrappDataShift.Code = currentMatricule;

            // Vérifiez l'existence de l'utilisateur dans la table Users
            var userExists = await _context.Users.AnyAsync(u => u.Code == scrappDataShift.Code);
            if (!userExists)
            {
                _logger.LogWarning($"Matricule {scrappDataShift.Code} does not exist.");
                return BadRequest("Le matricule spécifié n'existe pas.");
            }

            try
            {
                DateTime now = DateTime.Now;
                int shiftId = CalculateCurrentShift();

                scrappDataShift.Date = now;  // Enregistrer la date actuelle
                scrappDataShift.Shift = shiftId;

                _context.ScrappDataShift.Add(scrappDataShift);
                await _context.SaveChangesAsync();

                // Log the history after successful creation
                await LogScrappDataShiftAction(currentMatricule, "Insertion", "ScrappDataShift");

                _logger.LogInformation($"New ScrappDataShift with ID {scrappDataShift.Id} created.");
                return CreatedAtAction(nameof(GetScrappDataShifts), new { id = scrappDataShift.Id }, scrappDataShift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating ScrappDataShift.");
                return StatusCode(500, $"Erreur interne du serveur : {ex.Message}");
            }
        }



        [HttpGet("ByMatriculeToday/{matricule}")]
        public async Task<IActionResult> GetTodayScrappDataShiftByMatricule(int matricule)
        {
            try
            {
                var today = DateTime.Today;

                var scrappDataShift = await _context.ScrappDataShift
                    .Where(s => s.Date.Date == today && s.Code == matricule)
                    .Select(s => new
                    {
                        s.Id,
                        s.Date,
                        s.Code,
                        s.Purge,
                        s.DefautInjection,
                        s.DefautAssemblage,
                        s.Bavures,
                        s.Shift
                    }).ToListAsync();

                if (!scrappDataShift.Any())
                {
                    _logger.LogInformation($"No ScrappDataShift found for matricule {matricule} today.");
                    return Ok(new List<object>
            {
                new
                {
                    Id = 0,
                    Date = DateTime.Now,
                    Matricule = matricule,
                    Purge = 0,
                    DefautInjection = 0,
                    DefautAssemblage = 0,
                    Bavures = 0,
                    Shift = 0
                }
            });
                }

                _logger.LogInformation($"ScrappDataShift for matricule {matricule} today retrieved.");
                return Ok(scrappDataShift);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while retrieving today's ScrappDataShift for matricule {matricule}.");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des données.");
            }
        }

        [HttpGet("Today")]
        public async Task<IActionResult> GetTodayScrappData()
        {
            try
            {
                var today = DateTime.Today;

                var scrappData = await _context.ScrappData
                    .Include(s => s.User)
                    .Where(s => s.Date.Date == today)
                    .Select(s => new
                    {
                        s.Id,
                        s.Date,
                        s.Code,
                        s.QuantitéRetour,
                        s.QuantitéRestantePr,
                        
                        s.User.FirstName,
                        s.User.LastName
                    }).FirstOrDefaultAsync();

                var scrappDataShift = await _context.ScrappDataShift
                    .Include(s => s.User)
                    .Where(s => s.Date.Date == today)
                    .Select(s => new
                    {
                        s.Id,
                        s.Date,
                        s.Code,
                        s.Purge,
                        s.DefautInjection,
                        s.DefautAssemblage,
                        s.Bavures,
                        s.Shift,
                        s.User.FirstName,
                        s.User.LastName
                    }).ToListAsync();

                if (scrappData == null && !scrappDataShift.Any())
                {
                    _logger.LogInformation("No ScrappData or ScrappDataShift found for today.");
                    return Ok(new
                    {
                        scrappData = new
                        {
                            Id = 0,
                            Date = DateTime.Now,
                            Matricule = 0,
                            QuantitéRetour = 0,
                            QuantitéRestantePr = 0,
                            QuantitéEntreePr = 0,
                            Nom = "",
                            Prenom = ""
                        },
                        scrappDataShift = new[]
                        {
                            new
                            {
                                Id = 0,
                                Date = DateTime.Now,
                                Matricule = 0,
                                Purge = 0,
                                DefautInjection = 0,
                                DefautAssemblage = 0,
                                Bavures = 0,
                                Shift = 0,
                                Nom = "",
                                Prenom = ""
                            }
                        }
                    });
                }

                _logger.LogInformation("ScrappData and ScrappDataShift for today retrieved.");
                return Ok(new { scrappData, scrappDataShift });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving ScrappDataShift for today.");
                return StatusCode(500, $"Erreur interne du serveur : {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScrappDataShift(int id, [FromBody] ScrappDataShift scrappDataShift)
        {
            // Vérifiez que l'ID fourni dans l'URL correspond à l'ID de l'objet
            if (id != scrappDataShift.Id)
            {
                _logger.LogWarning($"ID mismatch. URL ID: {id}, Body ID: {scrappDataShift.Id}");
                return BadRequest("ID mismatch.");
            }

            // Trouvez l'objet existant dans la base de données
            var existingScrappDataShift = await _context.ScrappDataShift.FindAsync(id);
            if (existingScrappDataShift == null)
            {
                _logger.LogWarning($"ScrappDataShift with ID {id} not found.");
                return NotFound();
            }

            // Vérifiez si les nouvelles données sont différentes des anciennes
            bool isModified = false;

            if (existingScrappDataShift.Date != scrappDataShift.Date && scrappDataShift.Date != default)
            {
                existingScrappDataShift.Date = scrappDataShift.Date;
                isModified = true;
            }

            if (existingScrappDataShift.Purge != scrappDataShift.Purge && scrappDataShift.Purge != 0)
            {
                existingScrappDataShift.Purge = scrappDataShift.Purge;
                isModified = true;
            }

            if (existingScrappDataShift.DefautInjection != scrappDataShift.DefautInjection && scrappDataShift.DefautInjection != 0)
            {
                existingScrappDataShift.DefautInjection = scrappDataShift.DefautInjection;
                isModified = true;
            }

            if (existingScrappDataShift.DefautAssemblage != scrappDataShift.DefautAssemblage && scrappDataShift.DefautAssemblage != 0)
            {
                existingScrappDataShift.DefautAssemblage = scrappDataShift.DefautAssemblage;
                isModified = true;
            }

            if (existingScrappDataShift.Bavures != scrappDataShift.Bavures && scrappDataShift.Bavures != 0)
            {
                existingScrappDataShift.Bavures = scrappDataShift.Bavures;
                isModified = true;
            }

            if (existingScrappDataShift.Shift != scrappDataShift.Shift && scrappDataShift.Shift != 0)
            {
                existingScrappDataShift.Shift = scrappDataShift.Shift;
                isModified = true;
            }

            // Si aucune modification n'a été faite, on ne fait rien
            if (!isModified)
            {
                _logger.LogInformation($"No changes detected for ScrappDataShift with ID {id}. Update skipped.");
                return NoContent(); // Pas de modifications, rien à enregistrer
            }

            // Indiquez que l'entité a été modifiée
            _context.Entry(existingScrappDataShift).State = EntityState.Modified;

            try
            {
                // Sauvegardez les changements dans la base de données
                await _context.SaveChangesAsync();

                // Log the history after successful update
                var currentMatricule = GetCurrentUserMatricule();
                await LogScrappDataShiftAction(currentMatricule, "Modification", "ScrappDataShift");

                _logger.LogInformation($"ScrappDataShift with ID {id} updated successfully.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await ScrappDataShiftExists(id))
                {
                    _logger.LogWarning($"ScrappDataShift with ID {id} not found during update.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency exception during ScrappDataShift update.");
                    throw;
                }
            }

            return NoContent();
        }


        private async Task<bool> ScrappDataShiftExists(int id)
        {
            return await _context.ScrappDataShift.AnyAsync(e => e.Id == id);
        }





        [HttpGet("current-shift")]
        public IActionResult GetCurrentShift()
        {
            try
            {
                var shift = CalculateCurrentShift();
                return Ok(new { Shift = shift });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving current shift.");
                return StatusCode(500, $"Erreur interne du serveur : {ex.Message}");
            }
        }

        private int CalculateCurrentShift()
        {
            var now = DateTime.Now;

            if (now.Hour == 21 && now.Minute >= 45 || now.Hour > 21 || now.Hour < 5 || (now.Hour == 5 && now.Minute < 45))
            {
                return 1; // Shift 1
            }
            else if (now.Hour == 5 && now.Minute >= 45 || now.Hour > 5 && now.Hour < 13 || (now.Hour == 13 && now.Minute < 45))
            {
                return 2; // Shift 2
            }
            else if (now.Hour == 13 && now.Minute >= 45 || now.Hour > 13 && now.Hour < 21 || (now.Hour == 21 && now.Minute < 45))
            {
                return 3; // Shift 3
            }
            else
            {
                throw new Exception("L'heure actuelle ne correspond à aucun shift.");
            }
        }

        private async Task LogScrappDataShiftAction(int userCode, string actionType, string tableType)
        {
            var history = new ScrappDataHistory
            {
                DateTime = DateTime.Now,
                UserCode = userCode,
                ActionType = actionType,
                TableType = tableType
            };

            await _context.ScrappDataHistory.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        [HttpGet("ByDate")]
        public async Task<ActionResult<IEnumerable<ScrappDataShift>>> GetScrappDataShiftByDate(DateTime date)
        {
            var scrappDataShift = await _context.ScrappDataShift
                .Where(s => s.Date.Date == date.Date)
                
                .ToListAsync();

            if (scrappDataShift == null || !scrappDataShift.Any())
            {
                return Ok(new List<ScrappDataShift>());
            }

            return Ok(scrappDataShift);
        }
    


[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScrappDataShift(int id)
        {
            var scrappDataShift = await _context.ScrappDataShift.FindAsync(id);
            if (scrappDataShift == null)
            {
                _logger.LogWarning($"ScrappDataShift with ID {id} not found.");
                return NotFound();
            }

            _context.ScrappDataShift.Remove(scrappDataShift);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"ScrappDataShift with ID {id} deleted successfully.");
            return NoContent();
        }
    }
}





