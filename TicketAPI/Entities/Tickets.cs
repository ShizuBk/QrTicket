namespace TicketAPI.Entities
{
    public class Tickets
    {
        public Guid Id { get; set; }
        public string File { get; set; }
        public string Titular { get; set; }
        public int AssistantNum { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string SysPath { get; set; }
    }
}
