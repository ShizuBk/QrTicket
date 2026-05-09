using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketAPI.Entities
{
    public class Events
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public decimal Fee { get; set; }
        public DateTime SysDate { get; set; } = DateTime.Now;  
        public DateTime SysUpdate {  get; set; } = DateTime.Now;
        public bool SysVisible { get; set; } = true;
        public bool SysEnabled { get; set; }
        public string ImageUrl { get; set; }
        
        [NotMapped]
        public int SoldTickets { get; set; }

    }
}
