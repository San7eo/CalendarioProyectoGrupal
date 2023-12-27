using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReservaRestaurant.Domain.DTO;
using ReservaRestaurant.Domain.Entities;
using ReservaRestaurant.Repository.Interfaces;
using System.Globalization;

namespace ReservaRestaurant.Repository
{
    public class ReservaRepository : IReservaRepository
    {
        //private Guid Id { get; set; }
        // Id = Guid.NewGuid();

        private readonly ReservaRestaurantContext _reservaContext;
        public ReservaRepository(ReservaRestaurantContext context)
        {
            _reservaContext = context;
        }

        public async Task<bool> AddReserva(ReservaDTO reserva)
        {
            var newReserva = new Reserva();
            int rows = 0;
            int espacioDisponible = 0;
            DateTime fechaAhora = DateTime.Now;
            DateTime fechaReserva;

            Console.WriteLine("Iniciando AddReserva...");
            if (ComprobacionGeneral(reserva))
            {
                Console.WriteLine("ComprobacionGeneral pasa.");

                if (await ComprobacionSobreFecha(reserva.FechaReserva, fechaAhora)) 
                {
                    Console.WriteLine("ComprobacionSobreFecha pasa.");

                    fechaReserva = ConversionDateTime(reserva.FechaReserva);

                    bool tieneReservaExistente = await TieneReservaExistenteEnFecha(reserva.Dni, fechaReserva);

                    espacioDisponible = EspacioDisponible(fechaReserva, reserva.IdRangoReserva);

                    if (!tieneReservaExistente &&  espacioDisponible <= reserva.CantidadPersonas  ) 
                    {
                        Console.WriteLine("Condiciones para crear reserva cumplidas.");

                        newReserva.CodReserva = Guid.NewGuid().ToString();
                        newReserva.NombrePersona = reserva.NombrePersona;
                        newReserva.ApellidoPersona = reserva.ApellidoPersona;
                        newReserva.Dni = reserva.Dni;
                        newReserva.Mail = reserva.Mail;
                        newReserva.Celular = reserva.Celular;
                        newReserva.FechaReserva = fechaReserva;
                        newReserva.IdRangoReserva = reserva.IdRangoReserva;
                        newReserva.CantidadPersonas = reserva.CantidadPersonas;
                        newReserva.FechaAlta = fechaAhora;
                        newReserva.FechaModificacion = fechaAhora;
                        newReserva.Estado = "CONFIRMADO";

                        await _reservaContext.AddAsync(newReserva);
                        rows = await _reservaContext.SaveChangesAsync();
                    }
                }    
            }
            return rows > 0;
        }

        public async Task<bool> CancelarReserva(string dni, string fechaReserva, int idRango)
        {
               int rows = 0;
               DateTime FechaReserva;

               FechaReserva = ConversionDateTime(fechaReserva);
               
               if (await TieneReservaExistenteEnFecha(dni, FechaReserva)) 
               {

                     var reservaMatch = await _reservaContext.Reservas.FirstOrDefaultAsync(f => f.Dni == dni
                                                                                        && f.FechaReserva.Date == FechaReserva.Date
                                                                                        && f.IdRangoReserva == idRango);

                     reservaMatch.Estado = "CANCELADO";
                     reservaMatch.CantidadPersonas = 0;
                     reservaMatch.FechaModificacion = DateTime.Now;
                     rows = await _reservaContext.SaveChangesAsync();
               }
            return rows > 0;
        }

        public async Task<bool> UpdateReserva( string dni, string fechaAnterior, string fechaActual, int rango, int cantidadPersonas)
        {
            int rows = 0;
            DateTime FechaReservaAnterior;
            DateTime FechaReservaActual;

            FechaReservaAnterior = ConversionDateTime(fechaAnterior);
            FechaReservaActual = ConversionDateTime(fechaActual);

            if (await TieneReservaExistenteEnFecha(dni, FechaReservaAnterior) && (EspacioDisponible(FechaReservaAnterior, rango) <= cantidadPersonas) )
            {
                var reservaMatch = await _reservaContext.Reservas.FirstOrDefaultAsync(f => f.Dni == dni
                                                                   && f.FechaReserva.Date == FechaReservaAnterior);
                reservaMatch.FechaReserva = FechaReservaActual;
                reservaMatch.FechaModificacion = DateTime.Now;
                reservaMatch.IdRangoReserva = rango;
                reservaMatch.CantidadPersonas = cantidadPersonas;
                rows = await _reservaContext.SaveChangesAsync();
            }

            return rows > 0;
        }

        public async Task<List<CalendarioDTO>> GetListarTurnosDisponibles()
        {
            var calendario = new List<CalendarioDTO>();

            var FechaInicio = DateTime.Now;

            for(int i = 0; i < 7; i++)
            {
                var fechaActual = FechaInicio.AddDays(i);
                var diaSemana = fechaActual.ToString("dddd", new CultureInfo("es-ES")).ToLower();

                var rangos = await _reservaContext.RangoReservas.Select(rango => new RangoDTO
                {
                    Rango = rango.Descripcion,
                    Reserva = new ReservaInfoDTO
                    {
                        Ocupados = _reservaContext.Reservas
                                            .Where(reserva =>
                                                reserva.FechaReserva.Date == fechaActual.Date &&
                                                reserva.IdRangoReserva == rango.IdRangoReserva &&
                                                reserva.Estado.ToUpper() == "CONFIRMADO")
                                            .Sum(reserva => reserva.CantidadPersonas),

                        Libres = rango.Cupo - _reservaContext.Reservas
                                            .Where(reserva =>
                                                reserva.FechaReserva.Date == fechaActual.Date &&
                                                reserva.IdRangoReserva == rango.IdRangoReserva &&
                                                reserva.Estado.ToUpper() == "CONFIRMADO")
                                            .Sum(reserva => reserva.CantidadPersonas),

                        Total = rango.Cupo
                    }
                }).ToListAsync();

                calendario.Add(new CalendarioDTO
                {
                    Fecha = fechaActual.ToString("dd-MM-yyyy"),
                    Dia = diaSemana,
                    Rangos = rangos
                });
            }

            return calendario;
        }

        public async Task<List<CalendarioDTO>> GetListarSinTurnosDisponibles()
        {
            var calendario = new List<CalendarioDTO>();

            var FechaInicio = DateTime.Now;

            for (int i = 0; i < 7; i++)
            {
                var fechaActual = FechaInicio.AddDays(i);
                var diaSemana = fechaActual.ToString("dddd", new CultureInfo("es-ES")).ToLower();

                var rangos = await _reservaContext.RangoReservas
                    .Where(rango => _reservaContext.Reservas
                        .Where(reserva =>
                            reserva.FechaReserva.Date == fechaActual.Date &&
                            reserva.IdRangoReserva == rango.IdRangoReserva &&
                            reserva.Estado.ToUpper() == "CONFIRMADO")
                        .Sum(reserva => reserva.CantidadPersonas) == rango.Cupo)
                    .Select(rango => new RangoDTO
                    {
                        Rango = rango.Descripcion,
                        Reserva = new ReservaInfoDTO
                        {
                            Ocupados = _reservaContext.Reservas
                                .Where(reserva =>
                                    reserva.FechaReserva.Date == fechaActual.Date &&
                                    reserva.IdRangoReserva == rango.IdRangoReserva &&
                                    reserva.Estado.ToUpper() == "CONFIRMADO")
                                .Sum(reserva => reserva.CantidadPersonas),
                            Total = rango.Cupo,
                            Mensaje = "No tiene cupos disponibles"
                        }
                    }).ToListAsync();

                if (rangos.Any())
                {
                    calendario.Add(new CalendarioDTO
                    {
                        Fecha = fechaActual.ToString("dd-MM-yyyy"),
                        Dia = diaSemana,
                        Rangos = rangos
                    });
                }
            }
            return calendario;
        }

        public async Task<List<CalendarioDTO>> GetListarTurnosCancelados()
        {
            var calendario = new List<CalendarioDTO>();

            var FechaInicio = DateTime.Now;

            for (int i = 0; i < 7; i++)
            {
                var fechaActual = FechaInicio.AddDays(i);
                var diaSemana = fechaActual.ToString("dddd", new CultureInfo("es-ES")).ToLower();

                var rangos = await _reservaContext.RangoReservas
                    .Where(rango => _reservaContext.Reservas
                        .Any(reserva =>
                            reserva.FechaReserva.Date == fechaActual.Date &&
                            reserva.IdRangoReserva == rango.IdRangoReserva &&
                            reserva.Estado.ToUpper() == "CANCELADO"))
                    .Select(rango => new RangoDTO
                    {
                        Rango = rango.Descripcion,
                        Reserva = new ReservaInfoDTO
                        {
                            Ocupados = _reservaContext.Reservas
                                .Where(reserva =>
                                    reserva.FechaReserva.Date == fechaActual.Date &&
                                    reserva.IdRangoReserva == rango.IdRangoReserva &&
                                    reserva.Estado.ToUpper() == "CANCELADO")
                                .Sum(reserva => reserva.CantidadPersonas),
                            Total = rango.Cupo,
                            Mensaje = "CANCELADO"
                        }
                    }).ToListAsync();

                if (rangos.Any())
                {
                    calendario.Add(new CalendarioDTO
                    {
                        Fecha = fechaActual.ToString("dd-MM-yyyy"),
                        Dia = diaSemana,
                        Rangos = rangos
                    });
                }
            }

            return calendario;
        }

        public async Task<List<CalendarioDTO>> GetListarTurnosConfirmados()
        {
            var calendario = new List<CalendarioDTO>();

            var FechaInicio = DateTime.Now;

            for (int i = 0; i < 7; i++)
            {
                var fechaActual = FechaInicio.AddDays(i);
                var diaSemana = fechaActual.ToString("dddd", new CultureInfo("es-ES")).ToLower();

                var rangos = await _reservaContext.RangoReservas
                    .Where(rango => _reservaContext.Reservas
                        .Any(reserva =>
                            reserva.FechaReserva.Date == fechaActual.Date &&
                            reserva.IdRangoReserva == rango.IdRangoReserva &&
                            reserva.Estado.ToUpper() == "CONFIRMADO"))
                    .Select(rango => new RangoDTO
                    {
                        Rango = rango.Descripcion,
                        Reserva = new ReservaInfoDTO
                        {
                            Ocupados = _reservaContext.Reservas
                                .Where(reserva =>
                                    reserva.FechaReserva.Date == fechaActual.Date &&
                                    reserva.IdRangoReserva == rango.IdRangoReserva &&
                                    reserva.Estado.ToUpper() == "CONFIRMADO")
                                .Sum(reserva => reserva.CantidadPersonas),
                            Total = rango.Cupo,
                            Libres = rango.Cupo - _reservaContext.Reservas
                                .Where(reserva =>
                                    reserva.FechaReserva.Date == fechaActual.Date &&
                                    reserva.IdRangoReserva == rango.IdRangoReserva &&
                                    reserva.Estado.ToUpper() == "CONFIRMADO")
                                .Sum(reserva => reserva.CantidadPersonas),
                                    Mensaje = "CONFIRMADO"
                        }
                    }).ToListAsync();

                if (rangos.Any())
                {
                    calendario.Add(new CalendarioDTO
                    {
                        Fecha = fechaActual.ToString("dd-MM-yyyy"),
                        Dia = diaSemana,
                        Rangos = rangos
                    });
                }
            }

            return calendario;
        }
        public DateTime ConversionDateTime(string fecha)
        {
            DateTime fechaConvertida;

            fechaConvertida = DateTime.ParseExact(fecha, "dd/MM/yyyy", null);

            return fechaConvertida;
        }

        public bool ComprobacionGeneral(ReservaDTO reserva)
        {
            bool valido = false;

            if(!reserva.NombrePersona.IsNullOrEmpty() && !reserva.ApellidoPersona.IsNullOrEmpty() && !reserva.Dni.IsNullOrEmpty() && !reserva.Mail.IsNullOrEmpty() &&
                !reserva.Celular.IsNullOrEmpty() && !ComprobacionIdRango(reserva.IdRangoReserva, reserva.CantidadPersonas) && !reserva.FechaReserva.IsNullOrEmpty())
            {
               valido = true;
            }
            return valido;
        }

        public bool ComprobacionIdRango(int rango, int cantPersonas)
        {
            bool valido = true;

            if ( (rango > 0 && rango < 5) && (cantPersonas > 0 && cantPersonas <= 100) ) { valido = false; }
            
            return valido;
        }

        public async Task<bool> ComprobacionSobreFecha(string fechaIngresada, DateTime fechaAhora)
        {
            bool valido = false;
            

            if (DateTime.TryParseExact(fechaIngresada, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime fechaReserva))
            {
                if (fechaReserva >= fechaAhora)
                {
                    TimeSpan diferenciaDeSemana = fechaReserva - fechaAhora;

                    if (diferenciaDeSemana.TotalDays != 7)
                    {
                        valido = true;
                    }
                }
            }

            return valido;
        }

        public async Task<bool> TieneReservaExistenteEnFecha(string dni, DateTime fechaReserva)
        {

            var reservaExistente = await _reservaContext.Reservas
                                                                .FirstOrDefaultAsync(r => r.Dni == dni
                                                                                  && r.FechaReserva.Date == fechaReserva.Date
                                                                                  && r.Estado.ToUpper() == "CONFIRMADO");


            return reservaExistente != null;
        }

        public int EspacioDisponible(DateTime fechaReserva, int idRango)
        {
            var reservasConfirmadas = _reservaContext.Reservas
                                                              .Where(r => r.FechaReserva.Date == fechaReserva.Date
                                                                       && r.IdRangoReserva == idRango
                                                                       && r.Estado.ToUpper() == "CONFIRMADO");

            int cantidadPersonas = reservasConfirmadas.Sum(r => r.CantidadPersonas);

            int cupoMaximo = _reservaContext.RangoReservas.Where(r => r.IdRangoReserva == idRango)
                                                          .Select(r => r.Cupo)
                                                          .FirstOrDefault();

            int espacioDisponible = cupoMaximo - cantidadPersonas;

            return espacioDisponible;
        }
    }
}
