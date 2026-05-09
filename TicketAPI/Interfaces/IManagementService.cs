using Org.BouncyCastle.Tsp;
using TicketAPI.Dto;
using TicketAPI.Entities;

namespace TicketAPI.Interfaces
{
    public interface IManagementService
    {
        public Task DeleteEvent(EventDeleteDto dto);
        public Task DeleteFee(FeesDeleteDto dto);
        public Task DeleteUser(UserDeleteDto dto);
        
        //public Task<List<EventDetailsResponseDto>> GetEvents();
        public Task<List<FeesResponseDto>> GetFees();
        public Task<List<UserResponseDto>> GetUsers();
        public Task NewEvent(EventDetailsPostDto eventDetails);
        public Task NewFee(FeePostDto fees);
        public Task<UserResponseDto> NewUser(NewUserDto dto);
        public Task UpdateFees(FeesUpdateDto fees);
        public Task UpdateUser(UserUpdateDto dto);
        public Task UpdatEvent(EventDetailsUpdateDto eventDetails);
        Task<List<EventDetailsResponseDto>> GetEventDetails();
        Task<List<EventDetailsResponseDto>> GetPublicEvents();
        Task UpdateEventImage(Guid eventId, string imageUrl);
        Task<object> GetEventByIdAsync(Guid id);

        /*Task UpdateEventAsync(object evento);*/
        Task<bool> UpdateEventAsync(EventDetailsUpdateDto dto);
        Task<List<EventDetailsResponseDto>> SearchEventsAsync(EventSearchFiltersDto filters);


    }
}
