using iText.IO.Util;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using System.Text;
using TicketAPI.Data;
using TicketAPI.Dto;
using TicketAPI.Entities;
using TicketAPI.Interfaces;
using TicketAPI.Mappers;

namespace TicketAPI.Services
{
    public class ManagementService : IManagementService
    {
        private readonly ApplicationDbContext _context;

        public ManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetEventByIdAsync(Guid id)
        {
            return await _context.Events.FindAsync(id); 
        }

        public async Task<bool> UpdateEventAsync(EventDetailsUpdateDto dto)
        {
            var ev = await _context.Events.FindAsync(dto.Id);
            
            if (ev == null) return false;

            ev.Name = dto.Name;
            ev.Details = dto.Details;
            ev.EventDate = DateTime.SpecifyKind(dto.EventDate, DateTimeKind.Utc);
            ev.Fee = dto.Fee;
            ev.Capacity = dto.Capacity;
            ev.SysVisible = dto.SysVisible;

            _context.Events.Update(ev);
            return await _context.SaveChangesAsync() > 0;
        }



        public async Task<List<EventDetailsResponseDto>> SearchEventsAsync(EventSearchFiltersDto filters)
        {
            var query = _context.Events.AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(filters.Term))
                query = query.Where(e => e.Name.ToLower().Contains(filters.Term.ToLower()));

            if (filters.IsVisible.HasValue)
                query = query.Where(e => e.SysVisible == filters.IsVisible.Value);

            // MAPEO AL DTO CORRECTO
            return await query
                .Select(e => new EventDetailsResponseDto 
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventDate = e.EventDate,
                    Fee = e.Fee,
                    Capacity = e.Capacity,
                    SoldTickets = e.SoldTickets,
                    SysVisible = e.SysVisible, 
                    ImageUrl = e.ImageUrl
                })
                .ToListAsync();
        }
        // --- MÉTODOS DE CONSULTA ---

        // Para el Administrador: Muestra TODO
        public async Task<List<EventDetailsResponseDto>> GetEventDetails()
        {
            try
            {
                return await _context.Events
                    .AsNoTracking()
                    .Where(e => !e.Name.StartsWith("[ELIMINADO]"))
                    .Select(e => new EventDetailsResponseDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Details = e.Details,
                        ImageUrl = e.ImageUrl,
                        EventDate = e.EventDate,
                        Capacity = e.Capacity,
                        Fee = e.Fee,
                        SoldTickets = _context.Tickets // Cálculo de tickets vendidos
                            .Where(t => t.EventId == e.Id)
                            .Sum(t => (int?)t.AssistantNum) ?? 0,
                        
                        SysVisible = e.SysVisible,
                        SysEnabled = e.SysEnabled,
                        //IsActive = e.SysEnabled
                    })
                    .OrderByDescending(e => e.EventDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetEventDetails: {ex.Message}");
                return new List<EventDetailsResponseDto>();
            }
        }

        // 2. Para la Web Pública: SOLO eventos con SysVisible = true
        public async Task<List<EventDetailsResponseDto>> GetPublicEvents()
        {
            try
            {
                return await _context.Events
                    .AsNoTracking()
                    .Select(e => new EventDetailsResponseDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Details = e.Details,
                        EventDate = e.EventDate,
                        Capacity = e.Capacity,
                        Fee = e.Fee,
                        ImageUrl = e.ImageUrl,
                        SoldTickets = _context.Tickets
                            .Where(t => t.EventId == e.Id)
                            .Sum(t => (int?)t.AssistantNum) ?? 0, 
                        
                        SysVisible = e.SysVisible,
                        SysEnabled = e.SysEnabled,
                        IsActive = e.SysEnabled
                    })
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetPublicEvents: {ex.Message}");
                return new List<EventDetailsResponseDto>();
            }
        }

        // --- MÉTODOS DE CREACIÓN ---

        public async Task NewEvent(EventDetailsPostDto eventDetails)
        {
            var result = eventDetails.ToEntity();
            result.Id = Guid.NewGuid();
            result.Capacity = eventDetails.Capacity; 
            result.Fee = eventDetails.Fee;
            result.SysVisible = eventDetails.SysVisible;
            result.SysEnabled = true;
            result.SysDate = DateTime.UtcNow;
            result.SysUpdate = DateTime.UtcNow;
            result.EventDate = result.EventDate.ToUniversalTime();

            try 
            {
                await _context.Events.AddAsync(result);
                await _context.SaveChangesAsync(); 
            }
            catch (Exception ex) 
            {
                Console.WriteLine("**************** ERROR DE BASE DE DATOS ****************");
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                throw; 
            }
        }

        public async Task NewFee(FeePostDto fees)
        {
            var result = fees.ToEntity();
            result.SysVisible = true;
            result.SysEnabled = true;
            result.SysDate = DateTime.UtcNow;
            result.SysUpdate = DateTime.UtcNow;

            await _context.Fees.AddAsync(result);
            await _context.SaveChangesAsync();
        }

        public async Task<UserResponseDto> NewUser(NewUserDto dto)
        {
            var authLevels = await _context.AuthLevel.AsNoTracking().ToListAsync();
            var entity = new Users
            {
                Id = Guid.NewGuid(),
                SysEnabled = true,
                LastName = dto.LastName,
                Name = dto.Name,
                Password = dto.Password,
                Surname = dto.Surname,
                SysVisible = true,
                SysUpdate = DateTime.UtcNow,
                SysDate = DateTime.UtcNow,
                AuthLevel = dto.AuthLevel,
                UserName = GenUserName(dto)
            };

            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Name = dto.Name,
                LastName = dto.LastName,
                Surname = dto.Surname,
                UserName = entity.UserName,
                AuthLevel = authLevels.Where(e => e.Id == dto.AuthLevel)
                    .Select(e => e.Level).FirstOrDefault() ?? "N/A"
            };
        }

        // --- MÉTODOS DE ACTUALIZACIÓN ---

        public async Task UpdatEvent(EventDetailsUpdateDto eventDetails)
        {
            var result = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventDetails.Id);
            if (result != null)
            {
                result.EventDate = eventDetails.EventDate.ToUniversalTime();
                result.Fee = eventDetails.Fee;
                result.Details = eventDetails.Details;
                result.Name = eventDetails.Name;
                result.SysUpdate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else { throw new Exception("Evento no encontrado."); }
        }

        public async Task UpdateFees(FeesUpdateDto fees)
        {
            var result = await _context.Fees.FirstOrDefaultAsync(e => e.Id == fees.Id);
            if (result != null)
            {
                result.Fee = fees.Fee;
                result.Type = fees.Type;
                result.SysUpdate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateUser(UserUpdateDto dto)
        {
            var result = await _context.Users.FirstOrDefaultAsync(e => e.Id == dto.Id);
            if (result != null)
            {
                result.AuthLevel = dto.AuthLevel;
                result.Name = dto.Name;
                result.LastName = dto.LastName;
                result.Password = dto.Password;
                result.Surname = dto.Surname;
                result.SysUpdate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // --- MÉTODOS DE ELIMINACIÓN (Borrado Lógico) ---
        public async Task DeleteEvent(EventDeleteDto dto)
        {
            var result = await _context.Events.FirstOrDefaultAsync(e => e.Id == dto.Id);
            if (result != null)
            {
                if (!result.Name.StartsWith("[ELIMINADO]")) {
                            result.Name = "[ELIMINADO] " + result.Name;
                        }
                
                result.SysEnabled = false;
                result.SysVisible = false;
                result.SysUpdate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteFee(FeesDeleteDto dto)
        {
            var result = await _context.Fees.FirstOrDefaultAsync(e => e.Id == dto.Id);
            if (result != null)
            {
                result.SysEnabled = false;
                result.SysVisible = false;
                result.SysUpdate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteUser(UserDeleteDto dto)
        {
            var result = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (result != null)
            {
                result.SysVisible = false;
                result.SysEnabled = false;
                result.SysUpdate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // --- MÉTODOS AUXILIARES ---

        public async Task<List<FeesResponseDto>> GetFees()
        {
            try
            {
                var result = await _context.Fees.AsNoTracking().ToListAsync();
                return result.ToDtoList();
            }
            catch { return new List<FeesResponseDto>(); }
        }

        public async Task<List<UserResponseDto>> GetUsers()
        {
            try
            {
                // 1. Filtra solo los que NO han sido borrados lógicamente
                var result = await _context.Users
                    .Where(u => u.SysEnabled == true) 
                    .AsNoTracking()
                    .ToListAsync();

                var authLevels = await _context.AuthLevel.AsNoTracking().ToListAsync();

                var response = result.Select(user => new UserResponseDto
                {
                    Id = user.Id, 
                    Name = user.Name,
                    LastName = user.LastName,
                    Surname = user.Surname,
                    UserName = user.UserName,
                    AuthLevel = authLevels.Where(a => a.Id == user.AuthLevel)
                                        .Select(a => a.Level).FirstOrDefault() ?? "N/A"
                }).ToList();

                return response;
            }
            catch 
            { 
                return new List<UserResponseDto>(); 
            }
        }   

        private string GenUserName(NewUserDto dto)
        {
            var nameChar = dto.Name[0];
            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes($"{dto.Name}{dto.LastName}{dto.Surname}")))
                .Substring(0, 4);
            return nameChar + dto.LastName + hash;
        }

        public async Task UpdateEventImage(Guid eventId, string imageUrl)
            {
                var ev = await _context.Events.FindAsync(eventId);
                if (ev != null)
                {
                    ev.ImageUrl = imageUrl; 
                    await _context.SaveChangesAsync();
                }
                else 
                {
                    throw new Exception("No se encontró el evento para asignar la imagen.");
                }
            }
    }
}