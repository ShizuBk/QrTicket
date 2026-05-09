using System.ComponentModel.DataAnnotations.Schema;
namespace TicketAPI.Entities
{
    public class Tickets
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public string Titular { get; set; }
        public int AssistantNum { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Fee { get; set; }
        public string SysPath { get; set; }
        public Guid EventId { get; set; }
        
        [ForeignKey("EventId")]
        public virtual Events Event { get; set; }

    }
}
