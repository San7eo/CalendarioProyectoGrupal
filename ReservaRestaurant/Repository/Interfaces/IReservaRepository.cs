using ReservaRestaurant.Domain.DTO;

namespace ReservaRestaurant.Repository.Interfaces
{
    public interface IReservaRepository
    {
        public Task<bool> AddReserva(ReservaDTO reserva);

        public Task<bool> CancelarReserva(string dni, string fechaReserva, int idRango);

        public Task<bool> UpdateReserva(string dni, string fechaAnterior, string fechaActual, int rango, int cantidadPersonas);

        public Task<List<CalendarioDTO>> GetListarTurnosDisponibles();

        public Task<List<CalendarioDTO>> GetListarSinTurnosDisponibles();

        public Task<List<CalendarioDTO>> GetListarTurnosCancelados();

        public Task<List<CalendarioDTO>> GetListarTurnosConfirmados();


    }
}
