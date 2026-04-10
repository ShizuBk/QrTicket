using TicketAPI.Interfaces;
using TicketAPI.Dto;
using Microsoft.AspNetCore.Mvc;
using TicketAPI.Services.Processes;
using System.Collections.Generic;
using System;

namespace TicketAPI.Services
{
    public class TicketPurchaseService : ITicketPurchaseService
    {

        public byte[] GeneratePdfWithTemplate(TicketDetailsDto ticketDetailsDto)
        {
            var urlScan = "https://localhost:7251/scan";
            
            QrManager qrManager = new QrManager();
            byte[] qrBytes = qrManager.GetQrCodeBytes(urlScan, ticketDetailsDto);

            TicketManager ticketManager = new TicketManager();
            byte[] pdfBytes = ticketManager.CreatePDFTicket(qrBytes, ticketDetailsDto);

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                throw new Exception("El TicketManager no pudo generar contenido para el PDF.");
            }

            return pdfBytes;
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

        public FeeListDto GetFeeList()
        {
            return new FeeListDto(); 
        }

        public EventDetailsResponseDto GetEventDetails()
        {
            return new EventDetailsResponseDto();
        }
    }
}