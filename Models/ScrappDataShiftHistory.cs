using System;

namespace scrapp_app.Models
{
    public class ScrappDataShiftHistory
    {
        public int Id { get; set; }
        public int ScrappDataShiftId { get; set; }
        public DateTime Date { get; set; }
        public int Purge { get; set; }
        public int DéfautInjection { get; set; }
        public int DéfautAssemblage { get; set; }
        public int Bavures { get; set; }
        public int Shift { get; set; }
        public int Code { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ActionType { get; set; } // 'Insert', 'Update', 'Delete'
    }
}
