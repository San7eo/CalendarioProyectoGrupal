using System.Text;

namespace ReservaRestaurant.Reportes
{
    public class ResultadoOperacion
    {
        public bool Exitoso { get; set; }
        public StringBuilder MensajesError { get; set; }

        public ResultadoOperacion()
        {
            MensajesError = new StringBuilder();
            Exitoso = true; 
        }
    }
}
