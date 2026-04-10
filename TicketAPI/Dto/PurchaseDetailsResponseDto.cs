namespace TicketAPI.Dto
{
    public class PurchaseDetailsResponseDto
    {
        public string Titular {  get; set; }
        public List<AssistantDto> Assistants { get; set; }
        public string Fee { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
