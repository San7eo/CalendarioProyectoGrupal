using ReservaRestaurant.Domain.DTO;
using ReservaRestaurant.Reportes;

namespace ReservaRestaurant.Repository.Interfaces
{
    public interface IReservaRepository
    {
        public Task<ResultadoOperacion> AddReserva(ReservaDTO reserva);

        public Task<ResultadoOperacion> CancelarReserva(string dni, string fechaReserva, int idRango);

        public Task<ResultadoOperacion> UpdateReservaFecha(string dni, string fechaAnterior, string fechaActual);
        
        public Task<ResultadoOperacion> UpdateReservaRango(string dni, string fechaAnterior, int rango);
        
        public Task<ResultadoOperacion> UpdateReservaCantidadPersonas(string dni, string fechaAnterior, int cantidadPersonas);
        
        public Task<List<CalendarioDTO>> GetListarTurnosDisponibles();

        public Task<List<CalendarioDTO>> GetListarSinTurnosDisponibles();

        public Task<List<CalendarioDTO>> GetListarTurnosCancelados();

        public Task<List<CalendarioDTO>> GetListarTurnosConfirmados();


    }
}
