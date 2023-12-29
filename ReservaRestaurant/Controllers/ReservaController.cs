using Microsoft.AspNetCore.Mvc;
using ReservaRestaurant.Domain.DTO;
using ReservaRestaurant.Domain.Entities;
using ReservaRestaurant.Domain.Request;
using ReservaRestaurant.Reportes;
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

            if (!result.Exitoso)
            {
                return BadRequest(new { Message = result.MensajesError.ToString().Split("\n") });
            }

            return Created("", new { Message = "Se ha creado la reserva correctamente" });
        }
       
        [HttpPost("Cancelar-reserva")]
        public async Task<IActionResult> CancelarReservaController([FromBody] CancelarReservaRequest cancelacion)
        {
            var resultado = await _reservaService.CancelarReservaServiceAsync(cancelacion.Dni, cancelacion.FechaReserva, cancelacion.IdRango);

            if (!resultado.Exitoso)
            {
                return BadRequest(new { Message = resultado.MensajesError.ToString() });
            }

            return Ok(new { Message = "Se ha cancelado la reserva correctamente" });
        }

        /* [HttpPost("Actualizar-reserva")]
         public async Task<IActionResult> UpdateReservaController([FromBody] UpdateReservaRequest request)
         {
             var result = await _reservaService.UpdateReservaServiceAsync(request.Dni, request.FechaReservaAnterior, request.FechaReservaActual, request.Rango, request.CantidadPersonas);

             if (!result) return BadRequest(new { Message = "Se ha rechazado la actualizacion!" });

             return Ok(new { Message = "Se a actualizado la reserva correctamente" });
         }
        */
        [HttpPost("Actualizar-reserva-Fecha")]
        public async Task<IActionResult> ActualizarReservaFechaController(string dni, string fechaAnterior, string fechaActual)
        {
            var resultado = await _reservaService.UpdateReservaFechaServiceAsync(dni, fechaAnterior, fechaActual);

            if (!resultado.Exitoso)
            {
                return BadRequest(new { Message = resultado.MensajesError.ToString() });
            }

            return Ok(new { Message = "Se ha actualizado la fecha de la reserva correctamente" });
        }

        [HttpPost("Actualizar-reserva-Rango")]
        public async Task<IActionResult> ActualizarReservaRangoController(string dni, string fechaReserva, int rango)
        {
            var resultado = await _reservaService.UpdateReservaRangoServiceAsync(dni, fechaReserva, rango);

            if (!resultado.Exitoso)
            {
                return BadRequest(new { Message = resultado.MensajesError.ToString() });
            }

            return Ok(new { Message = "Se ha actualizado el rango de la reserva correctamente" });
        }
        [HttpPost("Actualizar-reserva-Cantidad-Personas")]
        public async Task<IActionResult> UpdateReservaCantidadPersonasController(string dni, string fechaAnterior, int cantidadPersonas)
        {
            var resultado = await _reservaService.UpdateReservaCantidadPersonasServiceAsync(dni, fechaAnterior, cantidadPersonas);

            if (!resultado.Exitoso)
            {
                return BadRequest(new { Message = resultado.MensajesError.ToString() });
            }

            return Ok(new { Message = "Se ha actualizado la cantidad de personas en la reserva correctamente" });
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
