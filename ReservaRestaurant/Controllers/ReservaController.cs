using Microsoft.AspNetCore.Mvc;
using ReservaRestaurant.Domain.DTO;
using ReservaRestaurant.Domain.Entities;
using ReservaRestaurant.Domain.Request;
using ReservaRestaurant.Service.Interfaces;

namespace ReservaRestaurant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservaController : ControllerBase
    {
        private IReservaService _reservaService;
        public ReservaController(IReservaService reservaService) 
        { 
            _reservaService = reservaService;
        }

        [HttpPost("Crear-reserva")]
        public async Task<IActionResult> AddReservaController([FromBody] ReservaDTO reserva)
        {
            var result = await _reservaService.AddReservaServiceAsync(reserva);

            if (!result) return BadRequest(new { Message = "Se ha rechazado la reserva!" });

            return Created("", new { Message = "Se a creado la reserva correctamente" });
        }

        [HttpPost("Cancelar-reserva")]
        public async Task<IActionResult> CancelarReservaController([FromBody] CancelarReservaRequest cancelacion)
        {
            var result = await _reservaService.CancelarReservaServiceAsync(cancelacion.Dni, cancelacion.FechaReserva, cancelacion.IdRango);

            if (!result) return BadRequest(new { Message = "Se ha rechazado la cancelacion!" });

            return Created("", new { Message = "Se a cancelado la reserva correctamente" });
        }

        [HttpPost("Actualizar-reserva")]
        public async Task<IActionResult> UpdateReservaController([FromBody] UpdateReservaRequest request)
        {
            var result = await _reservaService.UpdateReservaServiceAsync(request.Dni, request.FechaReservaAnterior, request.FechaReservaActual, request.Rango, request.CantidadPersonas);

            if (!result) return BadRequest(new { Message = "Se ha rechazado la actualizacion!" });

            return Created("", new { Message = "Se a actualizado la reserva correctamente" });
        }

        [HttpGet("Calendario")]
        public async Task<IActionResult> GetListaTurnosDisponiblesController()
        {
            var result = await _reservaService.GetListarTurnosDisponiblesServiceAsync();

            return Ok(result);

        }

        [HttpGet("Calendario-Sin-Cupos")]
        public async Task<IActionResult> GetListaSinTurnosDisponiblesController()
        {
            var result = await _reservaService.GetListarSinTurnosDisponiblesServiceAsync();

            return Ok(result);

        }

        [HttpGet("Calendario-Turnos-Cancelados")]
        public async Task<IActionResult> GetListaTurnosCanceladosController()
        {
            var result = await _reservaService.GetListarTurnosCanceladosServiceAsync();

            return Ok(result);

        }

        [HttpGet("Calendario-Turnos-Confirmados")]
        public async Task<IActionResult> GetListaTurnosConfirmadoController()
        {
            var result = await _reservaService.GetListarTurnosConfirmadosServiceAsync();

            return Ok(result);

        }

    }
}
