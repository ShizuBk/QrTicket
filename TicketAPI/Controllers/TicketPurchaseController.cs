using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketAPI.Dto;
using TicketAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketPurchaseController : ControllerBase
    {
        private readonly ITicketPurchaseService _service;
        private readonly IManagementService _managementService;

        public TicketPurchaseController(ITicketPurchaseService ticketService, IManagementService managementService)
        {
            _service = ticketService;
            _managementService = managementService; 
        }

        [HttpPost("/checkout")]
        public async Task<IActionResult> Checkout([FromBody] TicketDetailsDto purchaseDetails) 
        {
            try
            {
                var ticketResult = await _service.GeneratePdfWithTemplate(purchaseDetails);
                var result = await _service.GetTicketById(ticketResult);

                return File(result.Data, "application/pdf", result.Name);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("/download")]
        public async Task<IActionResult> Download(Guid id) 
        {
            try
            {
                var result = await _service.GetTicketById(id);

                if(result == null)
                    return NotFound();

                return File(result.Data, "application/pdf", result.Name);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("/search")]
        public async Task<IActionResult> Search([FromQuery] TicketSearchDto ticketSearch)
        {
            try
            {
                var result = await _service.GetTicketBySearch(ticketSearch);

                if (result == null)
                    return NotFound("No se encontró el ticket buscado");

                return File(result.Data, "application/pdf", result.Name);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

     
        [HttpPost("/confirm")]
        public async Task<Guid> ConfirmPurchase(TicketDetailsDto ticketDetails)
        {
            // Ahora sí, el await es válido porque el servicio devuelve Task<Guid>
            var result = await _service.ConfirmPurchase(ticketDetails);
            return result;
        }

        [HttpGet("/fees")]
        public async Task<IActionResult> GetFees()
        {
            try
            {
                var result = await _service.GetFeeList();

                if(result == null || !result.Any())
                    return NotFound("No hay tarifas declaradas");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("/events")]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var result = await _service.GetEventDetails();

                if (result == null || !result.Any())
                    return NotFound("No hay eventos disponibles");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("public")] 
        public async Task<ActionResult<List<EventDetailsResponseDto>>> GetPublicEvents()
        {
            try 
            {
                var eventos = await _managementService.GetPublicEvents();
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener la cartelera", error = ex.Message });
            }
        }      

        [HttpGet("/scan/{file}")]
        public async Task<IActionResult> Scan(string file)
        {
            try
            {
                var result = await _service.VerifyToken(file);
                var page = GenValidationPage(result, file);

                return Content(page, "text/html");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private string GenValidationPage(bool valid, string file)
        {
            string color = valid ? "green" : "red";
            string message = valid ? "Ticket Válido" : "Ticket Inválido";
            string description = valid
                ? "Ticket verificado correctamente."
                : "Ticket no válido o fue utilizado anteriormente.";

            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>Verificar Ticket</title>
<style>
body{{
font-family: Arial, sans-serif;
display: flex;
justify-content: center;
align-items: center;
height: 100vh;
margin: 0;
background: #f0f0f0;
}}
.container {{
text-align: center;
padding: 40px;
border-radius: 10px;
background: white;
box-shadow: 0 4px 6px rgba(0,0,0,0.1);
border-top: 5px solid {color}
}}
h1 {{ 
color: {color};
margin-bottom: 20px;
}}
.file {{
background: #f5f5f5;
padding: 10px;
border-radius: 5px;
margin: 20px 0;
}}
</style>
</head>
<body>
<div class='container'>
<h1>{message}</h1>
<p>{description}</p>
<div class='file'>
<strong>Folio:</strong> {HtmlEncode(file)}
</div>
</div>
</body>
</html>
";
        }

        private string HtmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}