namespace ReservaRestaurant.Domain.DTO
{
    public class ReservaInfoDTO
    {
        public int Ocupados { get; set; }

        public int Libres {  get; set; }

        public int Total {  get; set; }

        public string Mensaje { get; set; } = string.Empty;


    }
}
