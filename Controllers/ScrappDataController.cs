




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
    public class ScrappDataController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ScrappDataController> _logger;

        public ScrappDataController(AppDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<ScrappDataController> logger)
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

            var CodeClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (CodeClaim != null && int.TryParse(CodeClaim.Value, out int Code))
            {
                _logger.LogInformation($"Current user matricule: {Code}");
                return Code;
            }
            _logger.LogWarning("Matricule utilisateur non trouvé.");
            throw new UnauthorizedAccessException("Matricule utilisateur non trouvé.");
        }

        [HttpGet]
        public async Task<IActionResult> GetScrappData()
        {
            try
            {
                var scrappDataList = await _context.ScrappData.ToListAsync();
                _logger.LogInformation("ScrappData retrieved successfully.");
                return Ok(scrappDataList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving ScrappData.");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des données.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScrappData(int id)
        {
            var scrappData = await _context.ScrappData.FindAsync(id);

            if (scrappData == null)
            {
                _logger.LogWarning($"ScrappData with ID {id} not found.");
                return NotFound();
            }

            _logger.LogInformation($"ScrappData with ID {id} retrieved.");
            return Ok(scrappData);
        }
        [HttpPost]
        public async Task<IActionResult> PostScrappData([FromBody] ScrappData scrappData)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid ScrappData model state.");
                return BadRequest(ModelState);
            }

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return Unauthorized();
            }

            var currentMatricule = GetCurrentUserMatricule();
            scrappData.Code = currentMatricule;

            // Utilisation de DateTime.Now pour obtenir la date et l'heure actuelles
            DateTime now = DateTime.Now;
            scrappData.Date = now;

            var userExists = await _context.Users.AnyAsync(u => u.Code == scrappData.Code);
            if (!userExists)
            {
                _logger.LogWarning($"Matricule {scrappData.Code} does not exist.");
                return BadRequest("Le matricule spécifié n'existe pas.");
            }

            await _context.ScrappData.AddAsync(scrappData);
            await _context.SaveChangesAsync();

            // Enregistrer l'historique de l'insertion
            await LogScrappDataAction(currentMatricule, "Insertion", "ScrappData");

            _logger.LogInformation($"New ScrappData with ID {scrappData.Id} created.");
            return CreatedAtAction(nameof(GetScrappData), new { id = scrappData.Id }, scrappData);
        }

        [HttpGet("quantiteEntreePrByDate")]
        public async Task<IActionResult> GetQuantiteEntreePrByDate(DateTime date)
        {
            // Appeler la procédure stockée
            var quantiteEntreePr = await _context.QuantiteEntree
                .FromSqlInterpolated($"EXEC GetValuesByDate {date}")
                .ToListAsync();

            if (quantiteEntreePr == null || !quantiteEntreePr.Any())
            {
                _logger.LogWarning($"No QuantitéEntreePr found for date {date.ToShortDateString()}.");
                // Retourner un tableau vide avec un code de succès 200
                return Ok(new List<QuantitéEntree>());
            }

            _logger.LogInformation($"QuantitéEntreePr for date {date.ToShortDateString()} retrieved.");
            return Ok(quantiteEntreePr);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> PutScrappData(int id, [FromBody] ScrappData scrappData)
        {
            if (id != scrappData.Id)
            {
                _logger.LogWarning($"ID mismatch. URL ID: {id}, Body ID: {scrappData.Id}");
                return BadRequest();
            }

            var existingScrappData = await _context.ScrappData.FindAsync(id);
            if (existingScrappData == null)
            {
                _logger.LogWarning($"ScrappData with ID {id} not found.");
                return NotFound();
            }

            // Vérifiez si les nouvelles données sont différentes des anciennes
            bool isModified = false;

            if (existingScrappData.Date != scrappData.Date)
            {
                existingScrappData.Date = scrappData.Date;
                isModified = true;
            }

            if (existingScrappData.QuantitéRetour != scrappData.QuantitéRetour)
            {
                existingScrappData.QuantitéRetour = scrappData.QuantitéRetour;
                isModified = true;
            }

            if (existingScrappData.QuantitéRestantePr != scrappData.QuantitéRestantePr)
            {
                existingScrappData.QuantitéRestantePr = scrappData.QuantitéRestantePr;
                isModified = true;
            }

        

            if (!isModified)
            {
                _logger.LogInformation($"No changes detected for ScrappData with ID {id}. Update skipped.");
                return NoContent(); // Pas de modifications, rien à enregistrer
            }

            _context.Entry(existingScrappData).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Enregistrer l'historique de la mise à jour uniquement si les données ont été modifiées
                var currentMatricule = GetCurrentUserMatricule();
                await LogScrappDataAction(currentMatricule, "Modification", "ScrappData");

                _logger.LogInformation($"ScrappData with ID {id} updated successfully.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await ScrappDataExists(id))
                {
                    _logger.LogWarning($"ScrappData with ID {id} not found during update.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency exception during ScrappData update.");
                    throw;
                }
            }

            return NoContent();
        }





        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScrappData(int id)
        {
            var scrappData = await _context.ScrappData.FindAsync(id);

            if (scrappData == null)
            {
                _logger.LogWarning($"ScrappData with ID {id} not found.");
                return NotFound();
            }

            _context.ScrappData.Remove(scrappData);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"ScrappData with ID {id} deleted.");
            return NoContent();
        }
        [HttpGet("byDate")]
        public async Task<IActionResult> GetScrappDataByDate(DateTime date)
        {
            var scrappData = await _context.ScrappData
                .Where(sd => sd.Date.Date == date.Date)
                .ToListAsync();

            if (scrappData == null || !scrappData.Any())
            {
                // Retourner un statut 200 avec un message personnalisé
                return Ok(new List<ScrappData>());
            }

            return Ok(scrappData);
        }


        private async Task LogScrappDataAction(int userCode, string actionType, string tableType)
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




        [HttpGet("IsEmpty")]
        public async Task<IActionResult> GetIsScrappDataEmpty()
        {
            var isEmpty = !await _context.ScrappData.AnyAsync();
            _logger.LogInformation($"ScrappData is empty: {isEmpty}");
            return Ok(isEmpty);
        }
        [HttpGet("Today")]
        public async Task<IActionResult> GetTodayScrappData()
        {
            var today = DateTime.UtcNow.Date;
            var endOfDay = today.AddDays(1).AddTicks(-1);

            var scrappData = await _context.ScrappData
                .Where(s => s.Date >= today && s.Date <= endOfDay)
                .Select(s => new
                {
                    s.Id,
                    s.Date,
                    s.Code,
                    s.QuantitéRetour,
                    s.QuantitéRestantePr,
                }).FirstOrDefaultAsync();

            var scrappDataShift = await _context.ScrappDataShift
                .Where(s => s.Date >= today && s.Date <= endOfDay)
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

            if (scrappData == null && !scrappDataShift.Any())
            {
                _logger.LogInformation("No ScrappData or ScrappDataShift found for today.");
                return Ok(new
                {
                    scrappData = new
                    {
                        Id = 0,
                        Date = DateTime.UtcNow,
                        Code = 0,
                        QuantitéRetour = 0,
                        QuantitéRestantePr = 0,
                        QuantitéEntreePr = 0
                    },
                    scrappDataShift = new[]
                    {
                new
                {
                    Id = 0,
                    Date = DateTime.UtcNow,
                    Code = 0,
                    Purge = 0,
                    DefautInjection = 0,
                    DefautAssemblage = 0,
                    Bavures = 0,
                    Shift = 0
                }
            }
                });
            }

            _logger.LogInformation("ScrappData and ScrappDataShift for today retrieved.");
            return Ok(new { scrappData, scrappDataShift });
        }

        private async Task<bool> ScrappDataExists(int id)
        {
            var exists = await _context.ScrappData.AnyAsync(e => e.Id == id);
            _logger.LogInformation($"ScrappDataExists check for ID {id}: {exists}");
            return exists;
        }
    }
}










