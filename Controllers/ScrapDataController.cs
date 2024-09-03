using Microsoft.AspNetCore.Mvc;

namespace scrap_app.Controllers
{
    public class ScrapDataController : Controller
	{
		private readonly AppDbContext _context;

		public ScrapDataController(AppDbContext context)
		{
			_context = context;
		}

		// GET: ScrapData
		public async Task<IActionResult> Index()
		{
			var scrapData = await _context.ScrapData.ToListAsync();
			return View(scrapData);
		}

		// GET: ScrapData/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var scrapData = await _context.ScrapData
				.FirstOrDefaultAsync(m => m.Id == id);
			if (scrapData == null)
			{
				return NotFound();
			}

			return View(scrapData);
		}

		// GET: ScrapData/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: ScrapData/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Date,QuantityEntered,QuantityRejected,Team,Reason")] ScrapData scrapData)
		{
			if (ModelState.IsValid)
			{
				_context.Add(scrapData);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(scrapData);
		}

		// GET: ScrapData/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var scrapData = await _context.ScrapData.FindAsync(id);
			if (scrapData == null)
			{
				return NotFound();
			}
			return View(scrapData);
		}

		// POST: ScrapData/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Date,QuantityEntered,QuantityRejected,Team,Reason")] ScrapData scrapData)
		{
			if (id != scrapData.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(scrapData);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ScrapDataExists(scrapData.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(scrapData);
		}

		// GET: ScrapData/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var scrapData = await _context.ScrapData
				.FirstOrDefaultAsync(m => m.Id == id);
			if (scrapData == null)
			{
				return NotFound();
			}

			return View(scrapData);
		}

		// POST: ScrapData/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var scrapData = await _context.ScrapData.FindAsync(id);
			_context.ScrapData.Remove(scrapData);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ScrapDataExists(int id)
		{
			return _context.ScrapData.Any(e => e.Id == id);
		}
	}
}
