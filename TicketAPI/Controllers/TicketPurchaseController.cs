using Microsoft.AspNetCore.Mvc;
using Npgsql.Replication;
using System.Text.Encodings.Web;
using TicketAPI.Dto;
using TicketAPI.Interfaces;
using TicketAPI.Services;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketPurchaseController : ControllerBase
    {

        private readonly ITicketPurchaseService _service;

        public TicketPurchaseController(ITicketPurchaseService ticketService)
        {
            _service = ticketService;   
        }

        [HttpPost("/checkout")]
        public IActionResult Checkout([FromBody] TicketDetailsDto purchaseDetails) 
        {
            var ticketResult = _service.GeneratePdfWithTemplate(purchaseDetails).Result; 

            var result = _service.GetTicketById(ticketResult).Result;

            return File(result.Data, "application/pdf", result.Name);
        }

        [HttpGet("/download")]
        public IActionResult Download(Guid id) 
        {
            var result = _service.GetTicketById(id);

            return File(result.Result.Data, "application/pdf", result.Result.Name);
        }

        [HttpGet("/search")]
        public IActionResult Search(TicketSearchDto ticketSearch)
        {
            var result = _service.GetTicketBySearch(ticketSearch);

            return File(result.Result.Data, "application/pdf", result.Result.Name);
        }

        [HttpPost("/confirm")]
        public Guid ConfirmPurchase(TicketDetailsDto ticketDetails)
        {
            var result = _service.ConfirmPurchase(ticketDetails);
            return result;
        }

        [HttpGet("/fees")]
        public async Task<ActionResult<List<FeesResponseDto>>> GetFees()
        {
            var result = await _service.GetFeeList();
            return Ok(result);
        }


        [HttpGet("/events")]
        public async Task<ActionResult<List<EventDetailsResponseDto>>> GetEvents()
        {
            var result = await _service.GetEventDetails();
            return Ok(result);
        }

        [HttpGet("/scan/{file}")]
        public async Task<IActionResult> Scan(string file)
        {
            var result = await _service.VerifyToken(file);
            var page = GenValidationPage(result,file);

            var acceptHeader = Request.Headers.Accept;
            Response.ContentType = "text/html; charset=utf-8";

            return Content(page,"text/html");
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
height: 100vh;
margin: 0;
background: #f0f0f0;
}}
.container {{
text-align: center;
padding: 40px;
border-radiius: 10px;
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
