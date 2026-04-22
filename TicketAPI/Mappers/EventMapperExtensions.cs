using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Org.BouncyCastle.Asn1.Mozilla;
using TicketAPI.Dto;
using TicketAPI.Entities;

namespace TicketAPI.Mappers
{
    public static class EventMapperExtensions
    {
        public static EventDetailsResponseDto ToDto(this Events entity)
        {
            if (entity == null) return null;

            return new EventDetailsResponseDto
            {
                EventDate = entity.EventDate,
                Details = entity.Details,
                Fee = entity.Fee,
                Id = entity.Id,
                Name = entity.Name,
            };
        }

        public static List<EventDetailsResponseDto> ToDtoList(this IEnumerable<Events> entities)
        {
            return entities?.Select(e => e.ToDto()).ToList() ?? new List<EventDetailsResponseDto>();
        }

        public static Events ToEntity(this EventDetailsPostDto dto)
        {
            if (dto == null) return null;

            return new Events
            {
                EventDate = dto.EventDate,
                Details = dto.Details,
                Name = dto.Name,
                Fee = dto.Fee,
            };
        }
    }
}
