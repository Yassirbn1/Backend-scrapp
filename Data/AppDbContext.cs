using System.Collections.Generic;
using YourNamespace.Models;

namespace YourNamespace.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<ScrapData> ScrapData { get; set; }
	}
}
