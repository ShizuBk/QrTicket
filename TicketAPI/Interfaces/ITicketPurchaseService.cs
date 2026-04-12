using TicketAPI.Dto;
using TicketAPI.Services;

namespace TicketAPI.Interfaces
{
    public interface ITicketPurchaseService
    {
        public PurchaseDetailsResponseDto Checkout(PurchaseDetailsDto purchaseDetails);
        public Task<List<FeesDto>> GetFeeList();
        public Task<List<EventDetailsResponseDto>> GetEventDetails();
        public Guid ConfirmPurchase(TicketDetailsDto ticketDetailsDto);
        public Task<Guid> GeneratePdfWithTemplate(TicketDetailsDto ticketDetails);
        public Task<bool> VerifyToken(string token);
        public Task<TicketResultDto> GetTicketById(Guid id);
        public Task<TicketResultDto> GetTicketBySearch(TicketSearchDto ticketSearch);
    }
}