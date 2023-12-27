namespace ReservaRestaurant.Domain.DTO
{
    public class CalendarioDTO
    {
        public string Fecha { get; set; }

        public string Dia { get; set; } = string.Empty;

        public List<RangoDTO> Rangos { get; set; }
    }
}
