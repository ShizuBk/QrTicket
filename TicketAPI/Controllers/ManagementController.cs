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
        public async Task<IActionResult> GetFees()
        {
            try
            {
                var result = await _service.GetFees();

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                    return NotFound("No hay tarifas declaradas");

                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost("management/new_fee")]
        public async Task<IActionResult> NewFee(FeePostDto fees)
        {
            try
            {
                await _service.NewFee(fees);
                return Ok();
            }
            catch(Exception ex) 
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPatch("management/update_fee")]
        public async Task<IActionResult> UpdteFee(FeesUpdateDto fees)
        {
            try
            {
                await _service.UpdateFees(fees);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpDelete("management/delete_fee")]
        public async Task<IActionResult> DeleteFee(FeesDeleteDto dto)
        {
            try
            {
                await _service.DeleteFee(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        //Manejo de eventos
        [HttpGet("managemet/events")]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var result = await _service.GetEvents();

                return Ok(result);
            }
            catch(Exception ex) 
            {
                if (ex is NullReferenceException)
                    return NotFound("No hay eventos disponibles");

                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost("management/new_event")]
        public async Task<IActionResult> NewEvent(EventDetailsPostDto eventDetails)
        {
            try
            {
                await _service.NewEvent(eventDetails);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPatch("maanagement/update_event")]
        public async Task<IActionResult> UpdateEvent(EventDetailsUpdateDto eventDetails)
        {
            try
            {
                await _service.UpdatEvent(eventDetails);
                return Ok();
            }
            catch(Exception ex) 
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpDelete("management/delete_event")]
        public async Task<IActionResult> DeleteEvent(EventDeleteDto dto)
        {
            try
            {
                await _service.DeleteEvent(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        //Manejo de usuarios
        [HttpPost("management/new_user")]
        public async Task<IActionResult> NewUser(NewUserDto dto)
        {
            try
            {
                var result = await _service.NewUser(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("management/users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var result = await _service.GetUsers();

                return Ok(result);
            }
            catch(Exception ex) 
            {
                if (ex is NullReferenceException)
                    return NotFound("No hy usuarios registrados");

                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPatch("management/update_user")]
        public async Task<IActionResult> UpdateUser(UserUpdateDto dto)
        {
            try
            {
                await _service.UpdateUser(dto);
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpDelete("management/delete_user")]
        public async Task<IActionResult> DeleteUser(UserDeleteDto dto)
        {
            try
            {
                await _service.DeleteUser(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}
