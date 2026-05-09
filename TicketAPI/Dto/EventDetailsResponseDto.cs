namespace TicketAPI.Dto
{
    public class EventDetailsResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public int SoldTickets { get; set; } 
        public bool SysVisible { get; set; }
        public bool SysEnabled { get; set; }
        public bool IsActive { get; set; }
        public decimal Fee { get; set; }
        public string? ImageUrl { get; set; }
    }
}
