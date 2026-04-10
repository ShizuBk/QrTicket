namespace TicketAPI.Entities
{
    public class Events
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime EventDate { get; set; }
        public int Fee { get; set; }
        public DateTime SysDate { get; set; }   
        public DateTime SysUpdate {  get; set; }
    }
}
