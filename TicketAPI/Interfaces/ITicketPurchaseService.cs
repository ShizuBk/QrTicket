using TicketAPI.Dto;
using TicketAPI.Services;
using TicketAPI.Entities;

namespace TicketAPI.Interfaces
{
    public interface ITicketPurchaseService
    {
        public PurchaseDetailsResponseDto Checkout(PurchaseDetailsDto purchaseDetails);
        public Task<List<FeesResponseDto>> GetFeeList();
        public Task<List<EventDetailsResponseDto>> GetEventDetails();
        Task<Guid> ConfirmPurchase(TicketDetailsDto ticketDetails);
        public Task<Guid> GeneratePdfWithTemplate(TicketDetailsDto ticketDetails);
        public Task<bool> VerifyToken(string token);
        public Task<TicketResultDto> GetTicketById(Guid id);
        public Task<TicketResultDto> GetTicketBySearch(TicketSearchDto ticketSearch);

        public Task<bool> CreateEvent(Events newEvent);
    }
}