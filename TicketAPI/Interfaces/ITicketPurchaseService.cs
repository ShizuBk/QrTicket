using TicketAPI.Dto;
using TicketAPI.Services;

namespace TicketAPI.Interfaces
{
    public interface ITicketPurchaseService
    {
        public PurchaseDetailsResponseDto Checkout(PurchaseDetailsDto purchaseDetails);
        public FeeListDto GetFeeList();
        public EventDetailsResponseDto GetEventDetails();
        public Guid ConfirmPurchase(TicketDetailsDto ticketDetailsDto);
        
        public byte[] GeneratePdfWithTemplate(TicketDetailsDto ticketDetails);
    }
}