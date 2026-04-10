namespace TicketAPI.Entities
{
    public class Access
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public bool IsEvent { get; set; }
        public bool SysEnabled { get; set; }
        public bool SysVisible { get; set; }
    }
}
