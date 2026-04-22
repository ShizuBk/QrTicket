namespace TicketAPI.Dto
{
    public class FeesUpdateDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public int Fee { get; set; }
    }
}
