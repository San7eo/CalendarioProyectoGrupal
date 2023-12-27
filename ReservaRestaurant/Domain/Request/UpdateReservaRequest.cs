namespace ReservaRestaurant.Domain.Request
{
    public class UpdateReservaRequest
    {
        public string Dni {  get; set; } = string.Empty;

        public string FechaReservaAnterior {  get; set; } = string.Empty;

        public string FechaReservaActual {  get; set; } = string.Empty;

        public int Rango { get; set; }

        public int CantidadPersonas { get; set; }
    }
}
