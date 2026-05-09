namespace TicketAPI.Dto
{
    public class EventDetailsPostDto
    {
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public decimal Fee { get; set; }
        public bool SysVisible { get; set; }
        
    }
}
