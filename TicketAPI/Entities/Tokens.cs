using System.ComponentModel.DataAnnotations.Schema; // Necesitas este using para el [ForeignKey]

namespace TicketAPI.Entities
{
    public class Tokens
    {
        public Guid Id { get; set; }
        public string File { get; set; }
        public string Token { get; set; }

        public Guid TicketId { get; set; } 

    
        [ForeignKey("TicketId")]
        public virtual Tickets Ticket { get; set; } 
    }
}