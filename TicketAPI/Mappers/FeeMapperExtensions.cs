
using TicketAPI.Dto;
using TicketAPI.Entities;

namespace TicketAPI.Mappers
{
    public static class FeeMapperExtensions
    {
        public static FeesResponseDto ToDto(this Fees entity)
        {
            if (entity == null) return null;

            return new FeesResponseDto
            {
                Fee = entity.Fee,
                Id = entity.Id,
                Type = entity.Type,
            };
        }

        public static List<FeesResponseDto> ToDtoList(this IEnumerable<Fees> entities)
        {
            return entities?.Select(e => e.ToDto()).ToList() ?? new List<FeesResponseDto>();
        }

        public static Fees ToEntity(this FeePostDto dto)
        {
            if (dto == null) return null;

            return new Fees
            {
                Fee = dto.Fee,
                Type = dto.Type,
            };
        }
    }
}
