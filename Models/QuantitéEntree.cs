using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace scrapp_app.Models
{
    public class QuantitéEntree
    {
        [Key]
        public DateTime Date { get; set; }

        public int Value { get; set; }
    }
}
