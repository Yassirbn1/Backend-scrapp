using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Assurez-vous que cette directive est incluse
using scrapp_app.Data;
using projetStage.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace scrapp_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScrappDataHistoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ScrappDataHistoryController> _logger;

        public ScrappDataHistoryController(AppDbContext context, ILogger<ScrappDataHistoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistoryData([FromQuery] DateTime date)
        {
            try
            {
                // Filtrer les données selon la date spécifiée
                var historyData = await _context.ScrappDataHistory
      .Where(h => h.DateTime.Date == date.Date)
      .ToListAsync();


                if (historyData.Count == 0)
                {
                    _logger.LogInformation("Aucune donnée d'historique trouvée pour la date spécifiée.");
                    return Ok(new { message = "Aucune donnée d'historique trouvée pour la date spécifiée." });
                }

                return Ok(historyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des données d'historique.");
                return StatusCode(500, "Une erreur est survenue lors de la récupération des données.");
            }
        }
    }
}
