namespace TicketAPI.Dto
{
    public class EventDetailsResponseDto
    {
        public Guid EventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public int Fee { get; set; }
    }
}
