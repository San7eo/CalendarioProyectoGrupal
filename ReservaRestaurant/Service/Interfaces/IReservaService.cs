using ReservaRestaurant.Domain.DTO;
using ReservaRestaurant.Reportes;

namespace ReservaRestaurant.Service.Interfaces
{
    public interface IReservaService
    {
       public Task<ResultadoOperacion> AddReservaServiceAsync(ReservaDTO reserva);

        public Task<ResultadoOperacion> CancelarReservaServiceAsync(string dni, string fechaReserva, int idRango);

        public Task<ResultadoOperacion> UpdateReservaFechaServiceAsync(string dni, string fechaAnterior, string fechaActual);

        public Task<ResultadoOperacion> UpdateReservaRangoServiceAsync(string dni, string fechaAnterior, int rango);
        public Task<ResultadoOperacion> UpdateReservaCantidadPersonasServiceAsync(string dni, string fechaAnterior, int cantidadPersonas);
        
        public Task<List<CalendarioDTO>> GetListarTurnosDisponiblesServiceAsync();

        public Task<List<CalendarioDTO>> GetListarSinTurnosDisponiblesServiceAsync();

        public Task<List<CalendarioDTO>> GetListarTurnosCanceladosServiceAsync();

        public Task<List<CalendarioDTO>> GetListarTurnosConfirmadosServiceAsync();
    }
}
