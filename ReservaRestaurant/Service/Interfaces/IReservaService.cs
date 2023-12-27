using ReservaRestaurant.Domain.DTO;

namespace ReservaRestaurant.Service.Interfaces
{
    public interface IReservaService
    {
        public Task<bool> AddReservaServiceAsync(ReservaDTO reserva);

        public Task<bool> CancelarReservaServiceAsync(string dni, string fechaReserva, int idRango);

        public Task<bool> UpdateReservaServiceAsync(string dni, string fechaAnterior, string fechaActual, int rango, int cantidadPersonas);

        public Task<List<CalendarioDTO>> GetListarTurnosDisponiblesServiceAsync();

        public Task<List<CalendarioDTO>> GetListarSinTurnosDisponiblesServiceAsync();

        public Task<List<CalendarioDTO>> GetListarTurnosCanceladosServiceAsync();

        public Task<List<CalendarioDTO>> GetListarTurnosConfirmadosServiceAsync();
    }
}
