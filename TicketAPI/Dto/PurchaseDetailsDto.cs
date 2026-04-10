namespace TicketAPI.Dto
{
    public class PurchaseDetailsDto
    {
        public string Titular { get; set; }
        public List<AssistantDto> Assistants { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
