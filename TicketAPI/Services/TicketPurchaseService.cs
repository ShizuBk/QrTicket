using TicketAPI.Interfaces;
using TicketAPI.Dto;
using Microsoft.AspNetCore.Mvc;
using TicketAPI.Services.Processes;
using System.Collections.Generic;
using System;
using TicketAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TicketAPI.Entities;
using Microsoft.Extensions.Configuration.UserSecrets;
using iText.Pdfua.Wtpdf;

namespace TicketAPI.Services
{
    public class TicketPurchaseService : ITicketPurchaseService
    {
        private readonly ApplicationDbContext _context;
        public TicketPurchaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> GeneratePdfWithTemplate(TicketDetailsDto ticketDetailsDto)
        {
            // --- REGLA: VALIDACIÓN DE MÁXIMO 4 BOLETOS ---
            if (ticketDetailsDto.AssistantNumber > 4)
            {
                throw new Exception("Límite excedido: Solo se permite la compra de un máximo de 4 boletos por persona.");
            }
            
            
            if (ticketDetailsDto.AssistantNumber <= 0)
            {
                throw new Exception("Cantidad inválida: Debe adquirir al menos 1 boleto.");
            }

            var server = _context.LocalConfig.FirstOrDefault(e => e.Device == "Server");
            var urlScan = $"{server.Address}:{server.Port}/scan";

            QrManager qrManager = new QrManager();
            byte[] qrBytes = qrManager.GetQrCodeBytes(urlScan, ticketDetailsDto);

            TicketManager ticketManager = new TicketManager();
            byte[] pdfBytes = ticketManager.CreatePDFTicket(qrBytes, ticketDetailsDto);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new Exception("El TicketManager no pudo generar contenido para el PDF.");
            }

            var localPath = _context.LocalConfig.FirstOrDefault(e => e.Device == "SavePath");
            var syspath = ticketManager.SaveLocalFile(localPath.Address, pdfBytes);
            var name = ticketManager.GetFileName();
            var id = Guid.NewGuid();
            var file = qrManager.GetFile();

            
            var evento = await _context.Events.FindAsync(ticketDetailsDto.EventId);
            if (evento == null) throw new Exception("El tipo de entrada seleccionado no existe.");

            
            if (!evento.SysEnabled)
            {
                throw new Exception("Lo sentimos, la venta para este evento o categoría está pausada temporalmente.");
            }

            if (evento.Capacity > 0)
            {
                var vendidos = await _context.Tickets
                    .Where(t => t.EventId == ticketDetailsDto.EventId)
                    .SumAsync(t => (int?)t.AssistantNum) ?? 0;

                if (vendidos + ticketDetailsDto.AssistantNumber > evento.Capacity)
                {
                    int lugaresDisponibles = evento.Capacity - vendidos;
                    throw new Exception($"¡Cupo insuficiente! Solo quedan {lugaresDisponibles} lugares disponibles.");
                }
            }

            await _context.Tickets.AddAsync(new Tickets()
            {
                Id = id,
                EventId = ticketDetailsDto.EventId, 
                AssistantNum = ticketDetailsDto.AssistantNumber,
                File = file,
                PurchaseDate = ticketDetailsDto.PurchaseDate,
                Titular = $"{ticketDetailsDto.TitularName} {ticketDetailsDto.TitularLastName} {ticketDetailsDto.TitularSurname}",
                SysPath = syspath,
                Name = name,
            });

            await TokenGeneration(file, id); 

            await _context.SaveChangesAsync();

            return id;
        }
        public Task<TicketResultDto> GetTicketById(Guid id)
        {
            var result = _context.Tickets.FirstOrDefault(t => t.Id == id);
            var data = GetPdfTicket(id).Result;
            var ticket = new TicketResultDto()
            {
                Data = data,
                Name = result.Name
            };

            return Task.FromResult(ticket);
        }
        public Task<TicketResultDto> GetTicketBySearch(TicketSearchDto ticketSearch)
        {
            var result = _context.Tickets.Where(t =>
                (t.Titular.Contains(ticketSearch.Titular) &&
                (t.AssistantNum == ticketSearch.AssistantNum) &&
                (t.PurchaseDate.Date == ticketSearch.PurchaseDate.Date)))
                .FirstOrDefault();

            var data = GetPdfTicket(result.Id).Result;
            var ticket = new TicketResultDto()
            {
                Data = data,
                Name = result.Name
            };

           return Task.FromResult(ticket);
        }
        public async Task<byte[]> GetPdfTicket(Guid guid)
        {
            var result = _context.Tickets.FirstOrDefault( t => t.Id == guid);
            var sysPath = result.SysPath;

            using (FileStream fs = new FileStream(sysPath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                await fs.ReadExactlyAsync(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        // 1. Agregamos el parámetro ticketId
        private async Task TokenGeneration(string file, Guid ticketId)
        {
            using (SHA256 sHA = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(file);
                var hash = sHA.ComputeHash(bytes);
                string result = Convert.ToHexString(hash).ToLower();

                Tokens token = new Tokens()
                {
                    Id = Guid.NewGuid(),
                    File = file,
                    Token = result,
                    TicketId = ticketId 
                };

                await _context.AddAsync(token);
                await _context.SaveChangesAsync();
            }
        }

        public PurchaseDetailsResponseDto Checkout(PurchaseDetailsDto purchaseDetails)
        {
            int totalFee = 0;
            var feeList = GetFeeListInternal(); 

            foreach (var item in purchaseDetails.Assistants)
            {
                
                if(feeList.TryGetValue(item.FeeType, out int value))
                {
                    totalFee += value;
                }
            }

            return new PurchaseDetailsResponseDto()
            {
                Assistants = purchaseDetails.Assistants,
                Fee = $"{totalFee}",
            };
        }

        private Dictionary<string, int> GetFeeListInternal()
        {
            return new Dictionary<string, int>
            {
                { "General", 100 },
                { "Estudiante", 20 },
                { "Local", 50 },
                { "INAPAM", 0 },
                { "Discapacidad", 0 }
            };
        }

        public async Task<Guid> ConfirmPurchase(TicketDetailsDto ticketDetailsDto)
        {
            var id = Guid.NewGuid();

            var nuevoTicket = new TicketAPI.Entities.Tickets 
            {
                Id = id,
                EventId = ticketDetailsDto.EventId,
                AssistantNum = ticketDetailsDto.AssistantNumber 
            };

            try 
            {
                await _context.Tickets.AddAsync(nuevoTicket);
                await _context.SaveChangesAsync();
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar en DB: {ex.Message}");
                throw;
            }
        }


        async Task<List<FeesResponseDto>> ITicketPurchaseService.GetFeeList()
        {

            var result = await _context.Fees
                .AsNoTracking()
                .ToListAsync();


            var list = result.Select(e => new FeesResponseDto
            {
                Fee = e.Fee,
                Id = e.Id,
                Type = e.Type,
            }).ToList();

            return list;
        }

        public async Task<List<EventDetailsResponseDto>> GetEventDetails()
        {
            return await _context.Events
                .AsNoTracking()
                .Select(e => new EventDetailsResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Details = e.Details,
                    EventDate = e.EventDate,
                    Fee = e.Fee,
                    Capacity = e.Capacity,
                    
                    IsActive = e.SysVisible && e.SysEnabled, 
                    SysVisible = e.SysVisible,
                    SysEnabled = e.SysEnabled,

                    SoldTickets = _context.Tickets
                        .Where(t => t.EventId == e.Id)
                        .Sum(t => (int?)t.AssistantNum) ?? 0
                })
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<List<EventDetailsResponseDto>> GetPublicEvents()
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.SysVisible)
                .Select(e => new EventDetailsResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Details = e.Details,
                    EventDate = e.EventDate,
                    Fee = e.Fee,
                    Capacity = e.Capacity,
                    SysVisible = e.SysVisible,
                    SysEnabled = e.SysEnabled,
                    ImageUrl = e.ImageUrl,
                    SoldTickets = _context.Tickets
                        .Where(t => t.EventId == e.Id)
                        .Sum(t => (int?)t.AssistantNum) ?? 0
                })
                .OrderBy(e => e.EventDate) 
                .ToListAsync();
        }

        public async Task<bool> CreateEvent(Events newEvent)
        {
            try
            {
                newEvent.Id = Guid.NewGuid();
                newEvent.SysDate = DateTime.UtcNow; 
                newEvent.SysUpdate = DateTime.UtcNow;
                newEvent.SysVisible = true;
                newEvent.SysEnabled = true;

                _context.Events.Add(newEvent);
                
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico en DB: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
        }


        public async Task<bool> VerifyToken(string file)
        {
            // 1. Buscamos el token y "viajamos" hasta el evento
            var result = await _context.Tokens
                .Include(t => t.Ticket)        // Cargamos los datos del Ticket
                    .ThenInclude(tk => tk.Event) // Y de ahí cargamos el Evento
                .FirstOrDefaultAsync(t => t.File == file);

            if (result == null) return false;

            // 2. ¿Ya fue usado? (Tu lógica de los ceros)
            if (result.Token == "0000000000000000000000000000000000000000000000000000000000000000")
                return false;

            // 3. ¿Es para hoy? 
            // Comparamos la fecha del evento con la fecha actual
            if (result.Ticket.Event.EventDate.Date != DateTime.Today)
            {
                // No es hoy, así que no lo dejamos pasar (pero no quemamos el ticket)
                return false; 
            }

            // 4. Si pasó todo, quemamos el token
            result.Token = "0000000000000000000000000000000000000000000000000000000000000000";
            await _context.SaveChangesAsync();
            
            return true;
        }


    }
}