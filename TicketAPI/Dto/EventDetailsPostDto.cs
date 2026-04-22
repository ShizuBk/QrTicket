namespace TicketAPI.Dto
{
    public class EventDetailsPostDto
    {
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime EventDate { get; set; }
        public int Fee { get; set; }
    }
}
