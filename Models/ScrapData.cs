using System;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models
{
	public class ScrapData
	{
		[Key]
		public int Id { get; set; }

		public DateTime Date { get; set; }

		public int QuantityEntered { get; set; }

		public int QuantityRejected { get; set; }

		public string Team { get; set; }

		public string Reason { get; set; }
	}
}
