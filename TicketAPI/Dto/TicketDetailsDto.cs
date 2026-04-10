namespace TicketAPI.Dto
{
    public class TicketDetailsDto
    {
        public string TitularName {  get; set; }
        public string TitularLastName { get; set; }
        public string TitularSurname { get; set; }
        public int AssistantNumber { get; set; }
        public List<string> AssistantDetails { get; set; }
        public string Email { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
