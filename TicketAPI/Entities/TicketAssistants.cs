namespace TicketAPI.Entities
{
    public class TicketAssistants
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public string FeeType { get; set; }
    }
}
