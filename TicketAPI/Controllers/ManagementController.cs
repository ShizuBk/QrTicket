using Microsoft.AspNetCore.Mvc;
using TicketAPI.Dto;
using TicketAPI.Interfaces;
using Microsoft.AspNetCore.Cors;

namespace TicketAPI.Controllers
{
    [ApiController]
    [Route("api/management")]
    [EnableCors("AllowAll")]
    public class ManagementController : ControllerBase
    {
        private readonly IManagementService _service;

        public ManagementController(IManagementService service)
        {
            _service = service;
        }

        // ==========================================
        // MANEJO DE TARIFAS
        // ==========================================

        [HttpGet("management/fees")]
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
            catch (Exception ex)
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

        // ==========================================
        // MANEJO DE EVENTOS
        // ==========================================

[HttpGet("events")]
public async Task<IActionResult> GetEvents()
{
    try
    {
        var result = await _service.GetEventDetails();

        // QUITAMOS el check de !result.Any() que lanza el NotFound
        if (result == null) 
        {
            return Ok(new List<EventDetailsResponseDto>()); // Devuelve lista vacía
        }

        return Ok(result); // Si la lista está vacía, devuelve [] con status 200
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error interno: {ex.Message}");
    }
}

    [HttpPost("new_event")] 
    public async Task<IActionResult> NewEvent([FromBody] EventDetailsPostDto dto)
    {
        try
        {
            await _service.NewEvent(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }


[HttpPut("update_event")]
public async Task<IActionResult> UpdateEvent([FromBody] EventDetailsUpdateDto dto)
{
    var success = await _service.UpdateEventAsync(dto);
    if (!success) return NotFound();
    return Ok(new { message = "Actualizado" });
}

[HttpPost("search")]
public async Task<IActionResult> SearchEvents([FromBody] EventSearchFiltersDto filters)
{
    // Llama al servicio, el cual ahora sí cumple con la interfaz
    var results = await _service.SearchEventsAsync(filters);
    return Ok(results);
}

    // Ruta final: POST api/management/update_status
    [HttpPost("update_status")]
    public async Task<IActionResult> UpdateStatus([FromBody] System.Text.Json.JsonElement data)
    {
        try
        {
            if (!data.TryGetProperty("Id", out var idProp)) return BadRequest("Falta ID");
            Guid eventId = Guid.Parse(idProp.GetString());

            // Usamos dynamic para evitar conflictos de tipos
            dynamic evento = await _service.GetEventByIdAsync(eventId); 
            if (evento == null) return NotFound();

            if (data.TryGetProperty("SysVisible", out var v)) evento.SysVisible = v.GetBoolean();
            if (data.TryGetProperty("SysEnabled", out var e)) evento.SysEnabled = e.GetBoolean();

            await _service.UpdateEventAsync(evento); 
            return Ok();
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

// QUITAMOS "management/" de la ruta y AGREGAMOS [FromBody]
[HttpDelete("delete_event")]
public async Task<IActionResult> DeleteEvent([FromBody] EventDeleteDto dto)
{
    try
    {
        // Verificamos que el DTO traiga el ID
        if (dto == null) return BadRequest("Datos inválidos");

        await _service.DeleteEvent(dto);
        return Ok();
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error interno: {ex.Message}");
    }
}



// Asegúrate de que el nombre del parámetro sea 'eventId' para que coincida con el {eventId} de la ruta
[HttpPost("upload-image/{eventId}")] 
public async Task<IActionResult> UploadImage(Guid eventId, IFormFile file)
{
    try
    {
        if (file == null || file.Length == 0)
            return BadRequest("No se seleccionó ninguna imagen.");

        // 1. Ruta física
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "events");
        
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // 2. Nombre de archivo único (usamos el ID para que cada evento tenga solo una foto)
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{eventId}{extension}"; 
        var filePath = Path.Combine(folderPath, fileName);

        // 3. Guardar archivo (FileMode.Create sobrescribe si ya existe)
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 4. Actualizar base de datos
        // Importante: Guardamos la ruta que usará el navegador (con slashes /)
        var relativePath = $"img/events/{fileName}";
        await _service.UpdateEventImage(eventId, relativePath);

        return Ok(new { path = relativePath });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error al subir imagen: {ex.Message}");
    }
}

        // ==========================================
        // MANEJO DE USUARIOS
        // ==========================================

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
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                    return NotFound("No hay usuarios registrados");

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
            catch (Exception ex)
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