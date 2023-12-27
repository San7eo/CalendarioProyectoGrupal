namespace ReservaRestaurant.Domain.DTO
{
    public class ReservaDTO
    {
        public string NombrePersona { get; set; } = string.Empty;
        public string ApellidoPersona { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string FechaReserva { get; set; } = string.Empty;
        public int IdRangoReserva { get; set; }
        public int CantidadPersonas { get; set; }

    }
}
