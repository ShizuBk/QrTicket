using Microsoft.AspNetCore.Mvc;
using Npgsql.Replication;
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
        public async Task<ActionResult<List<FeesDto>>> GetFees()
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

        [HttpGet("/scan/")]
        public async Task<IActionResult> Scan(string token)
        {
            var result = await _service.VerifyToken(token);

            return Ok(result);
        }
    }
}
