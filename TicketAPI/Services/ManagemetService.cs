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
    public class ManagemetService : IManagementService
    {
        private readonly ApplicationDbContext _context;
        public async Task DeleteEvent(EventDeleteDto dto)
        {
            var result = await _context.Events.Where( e => e.Id == dto.Id ).FirstOrDefaultAsync();
            result.SysEnabled = false;
            result.SysUpdate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteFee(FeesDeleteDto dto)
        {
            var result = await _context.Fees.Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
            result.SysEnabled = false;
            result.SysVisible = false;
            result.SysUpdate = DateTime.Now;
            
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(UserDeleteDto dto)
        {
            var result = await _context.Users.Where( e => e.Id != dto.Id ).FirstOrDefaultAsync();
            result.SysVisible = false;
            result.SysEnabled = false;
            result.SysUpdate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public Task<List<EventDetailsResponseDto>> GetEvents()
        {
            var result = _context.Events.AsNoTracking().ToList();
            var response = result.ToDtoList();

            return Task.FromResult(response);
        }

        public Task<List<FeesResponseDto>> GetFees()
        {
            var result = _context.Fees.AsNoTracking().ToList();
            var response = result.ToDtoList();

            return Task.FromResult(response);
        }

        public Task<List<UserResponseDto>> GetUsers()
        {
            var result = _context.Users.AsNoTracking().ToList();
            var authLevels = _context.AuthLevel.AsNoTracking().ToList();
            List<UserResponseDto> response = new();
            foreach(var user in result)
            {
                response.Add(new UserResponseDto
                {
                   Name = user.Name,
                   LastName = user.LastName,
                   Surname = user.Surname,
                   UserName = user.UserName,
                   AuthLevel = authLevels
                   .Where(e => e.Id == user.Id)
                   .Select(e => e.Level)
                   .First(),
                });
            }

            return Task.FromResult(response);
        }

        public async Task NewEvent(EventDetailsPostDto eventDetails)
        {
            var result = eventDetails.ToEntity();
            await _context.Events.AddAsync(result);
        }

        public async Task NewFee(FeePostDto fees)
        {
            var result = fees.ToEntity();
            await _context.Fees.AddAsync(result);
        }

        public Task<UserResponseDto> NewUser(NewUserDto dto)
        {
            var authLevels = _context.AuthLevel.AsNoTracking().ToList();
            var entity = new Users
            {
                Id = Guid.NewGuid(),
                SysEnabled = true,
                LastName = dto.LastName,
                Name = dto.Name,
                Password = dto.Password,
                Surname = dto.Surname,
                SysVisible = true,
                SysUpdate = DateTime.Now,
                SysDate = DateTime.Now,
                AuthLevel = dto.AuthLevel,
                UserName = GenUserName(dto)
            };

            _context.Users.Add(entity);

            return Task.FromResult(new UserResponseDto
            {
                Name = dto.Name,
                LastName = dto.LastName,
                Surname = dto.Surname,
                UserName = entity.UserName,
                AuthLevel = authLevels.Where(e => e.Id == dto.AuthLevel)
                .Select(e => e.Level).First()
            });
        }

        private string GenUserName(NewUserDto dto)
        {
            var nameChar = dto.Name[0];
            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes($"{dto.Name}{dto.LastName}{dto.Surname}")))
                .Substring(0,4);
            return nameChar + dto.LastName + hash;
        }

        public async Task UpdateFees(FeesUpdateDto fees)
        {
            var result = await _context.Fees.Where(e => e.Id == fees.Id).FirstOrDefaultAsync();
            
            result.Fee = fees.Fee;
            result.Type = fees.Type;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(UserUpdateDto dto)
        {
            var result = await _context.Users.Where(e => e.Id == dto.Id).FirstOrDefaultAsync();
            
            result.AuthLevel = dto.AuthLevel;
            result.Name = dto.Name;
            result.LastName = dto.LastName;
            result.Password = dto.Password;
            result.Surname = dto.Surname;
            result.SysUpdate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task UpdatEvent(EventDetailsUpdateDto eventDetails)
        {
            var result = await _context.Events.Where(e => e.Id == eventDetails.Id).FirstOrDefaultAsync();

            result.EventDate = eventDetails.EventDate;
            result.Fee = eventDetails.Fee;
            result.Details = eventDetails.Details;
            result.Name = eventDetails.Name;
            result.SysUpdate = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
