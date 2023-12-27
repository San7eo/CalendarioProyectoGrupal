namespace ReservaRestaurant.Domain.Request
{
    public class CancelarReservaRequest
    {
        public string Dni { get; set; } = string.Empty;

        public string FechaReserva {  get; set; } = string.Empty;

        public int IdRango { get; set; }
    }
}
