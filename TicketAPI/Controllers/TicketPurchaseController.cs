using Microsoft.AspNetCore.Mvc;
using Npgsql.Replication;
using TicketAPI.Dto;
using TicketAPI.Interfaces;
using TicketAPI.Services;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketPurchaseController : ControllerBase
    {

        ITicketPurchaseService TicketService = new TicketPurchaseService();
        [HttpPost("/checkout")]
        public IActionResult Checkout([FromBody] TicketDetailsDto purchaseDetails) 
        {
            byte[] pdfArchivo = TicketService.GeneratePdfWithTemplate(purchaseDetails); 

            if (pdfArchivo == null || pdfArchivo.Length == 0)
            {
                return BadRequest("Error: El servidor generó un archivo vacío.");
            }

            // Forzamos el nombre con extensión y el tipo de contenido
            return File(pdfArchivo, "application/pdf", $"Ticket_{DateTime.Now.Ticks}.pdf");
        }
        [HttpPost("/confirm")]
        public Guid ConfirmPurchase(TicketDetailsDto ticketDetails)
        {
            var result = TicketService.ConfirmPurchase(ticketDetails);
            return result;
        }

        [HttpGet("/fees")]
        public IEnumerable<FeeListDto> GetFees()
        {
            var result = TicketService.GetFeeList();
            yield return result;
        }


        [HttpGet("/events")]
        public IEnumerable<EventDetailsResponseDto> GetEvents()
        {
            var result = TicketService.GetEventDetails();
            return (IEnumerable<EventDetailsResponseDto>)result;
        }
    }
}
