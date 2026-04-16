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
            var server = _context.LocalConfig.FirstOrDefault(e => e.Device == "Server");
            var urlScan = $"{server.Address}:{server.Port}/scan";

                //"https://localhost:7251/scan";
            
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

             _context.Tickets.AddAsync(new Tickets()
            {
                AssistantNum = ticketDetailsDto.AssistantNumber,
                File = file,
                PurchaseDate = ticketDetailsDto.PurchaseDate,
                Titular = $"{ticketDetailsDto.TitularName} {ticketDetailsDto.TitularLastName} {ticketDetailsDto.TitularSurname}",
                SysPath = syspath,
                Id = id,
                Name = name,
            });

            await TokenGeneration(file);

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
                await fs.ReadAsync(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        private async Task TokenGeneration(string file)
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
                    Token = result
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

        public Guid ConfirmPurchase(TicketDetailsDto ticketDetailsDto)
        {
            var id = Guid.NewGuid();
        
            
            return id;
        }


        async Task<List<FeesDto>> ITicketPurchaseService.GetFeeList()
        {

            var result = await _context.Fees
                .AsNoTracking()
                .ToListAsync();


            var list = result.Select(e => new FeesDto
            {
                Fee = e.Fee,
                Id = e.Id,
                Type = e.Type,
            }).ToList();

            return list;
        }

        public async Task<List<EventDetailsResponseDto>> GetEventDetails()
        {
            var result = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            var list = result.Select(e => new EventDetailsResponseDto
            {
                EventDate = e.EventDate,
                EventID = e.Id,
                EventName = e.Name,
                Fee = e.Fee
            }).ToList();

            return list;
        }

        public async Task<bool> VerifyToken(string file)
        {

            var result = await _context.Tokens.FirstOrDefaultAsync( t => t.File == file);

            if (result == null)
                return false;

            if (result.Token != "0000000000000000000000000000000000000000000000000000000000000000")
            {
                result.Token = "0000000000000000000000000000000000000000000000000000000000000000";
                await _context.SaveChangesAsync();
                return true;
            }
            else
                return false;
 
        }


    }
}