using iText.Pdfua.Checkers.Utils;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Tsp;
using TicketAPI.Dto;
using TicketAPI.Interfaces;

namespace TicketAPI.Controllers
{
    public class ManagementController : ControllerBase
    {
        private readonly IManagementService _service;

        public ManagementController(IManagementService service)
        {
            _service = service;
        }

        // Mnejo de tarifas
        [HttpGet("managemet/fees")]
        public async Task<ActionResult<List<FeesResponseDto>>> GetFees()
        {
            var result = await _service.GetFees();
            return Ok(result);
        }

        [HttpPost("management/new_fee")]
        public async Task<IActionResult> NewFee(FeePostDto fees)
        {
            await _service.NewFee(fees);
            return Ok();
        }

        [HttpPatch("management/update_fee")]
        public async Task<IActionResult> UpdteFee(FeesUpdateDto fees)
        {
            await _service.UpdateFees(fees);
            return Ok();
        }

        [HttpDelete("management/delete_fee")]
        public async Task<IActionResult> DeleteFee(FeesDeleteDto dto)
        {
            await _service.DeleteFee(dto);
            return Ok();
        }

        //Manejo de eventos
        [HttpGet("managemet/events")]
        public async Task<ActionResult<List<EventDetailsResponseDto>>> GetEvents()
        {
            var result = await _service.GetEvents();
            return Ok(result);
        }

        [HttpPost("management/new_event")]
        public async Task<IActionResult> NewEvent(EventDetailsPostDto eventDetails)
        {
            await _service.NewEvent(eventDetails);
            return Ok();
        }

        [HttpPatch("maanagement/update_event")]
        public async Task<IActionResult> UpdateEvent(EventDetailsUpdateDto eventDetails)
        {
            await _service.UpdatEvent(eventDetails);
            return Ok();
        }

        [HttpDelete("management/delete_event")]
        public async Task<IActionResult> DeleteEvent(EventDeleteDto dto)
        {
            await _service.DeleteEvent(dto);
            return Ok();
        }

        //Manejo de usuarios
        [HttpPost("management/new_user")]
        public async Task<UserResponseDto> NewUser(NewUserDto dto)
        {
            var result = await _service.NewUser(dto);
            return result;
        }

        [HttpGet("management/users")]
        public async Task<List<UserResponseDto>> GetUsers()
        {
            var result = await _service.GetUsers();
            return result;
        }

        [HttpPatch("management/update_user")]
        public async Task<IActionResult> UpdateUser(UserUpdateDto dto)
        {
            await _service.UpdateUser(dto);
            return Ok();
        }

        [HttpDelete("management/delete_user")]
        public async Task<IActionResult> DeleteUser(UserDeleteDto dto)
        {
            await _service.DeleteUser(dto);
            return Ok();
        }
    }
}
