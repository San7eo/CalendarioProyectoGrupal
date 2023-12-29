using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReservaRestaurant.Domain.DTO;
using ReservaRestaurant.Domain.Entities;
using ReservaRestaurant.Reportes;
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

        public async Task<ResultadoOperacion> AddReserva(ReservaDTO reserva)
        {
            var resultado = new ResultadoOperacion();
            var newReserva = new Reserva();
            int rows = 0;
            DateTime fechaAhora = DateTime.Now;
            DateTime fechaReserva;

            var comprobacionGeneral = await ComprobacionGeneral(reserva);
            fechaReserva = await ConversionDateTime(reserva.FechaReserva);
            var comprobacionSobreFecha = await ComprobacionSobreFecha(reserva.FechaReserva, fechaAhora); 
            var tieneReservaExistente = await TieneReservaExistenteEnFecha(reserva.Dni, fechaReserva);
            var espacioDisponibleResponse = await EspacioDisponible(fechaReserva, reserva.IdRangoReserva);

            if (!comprobacionGeneral.Exitoso)
                resultado.MensajesError.AppendLine(comprobacionGeneral.MensajesError.ToString());

            if (!comprobacionSobreFecha.Exitoso)
                resultado.MensajesError.AppendLine(comprobacionSobreFecha.MensajesError.ToString());

            if (espacioDisponibleResponse <= reserva.CantidadPersonas)
                resultado.MensajesError.AppendLine($"La cantidad de personas no puede ser mayor al espacio disponible. Espacio disponible: {espacioDisponibleResponse}.\n");

            if (resultado.MensajesError.Length == 0 && !tieneReservaExistente.Exitoso)
            {
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

            resultado.Exitoso = rows > 0;
            return resultado;
        }

        public async Task<ResultadoOperacion> CancelarReserva(string dni, string fechaReserva, int idRango)
        {
            var resultado = new ResultadoOperacion();
            int rows = 0;
            DateTime FechaReserva;

            FechaReserva = await ConversionDateTime(fechaReserva);

            var resultadoFecha = await TieneReservaExistenteEnFecha(dni, FechaReserva);

            if (!resultadoFecha.Exitoso)
            {
                return resultadoFecha;
            }

            var reservaMatch = await _reservaContext.Reservas.FirstOrDefaultAsync(f => f.Dni == dni
                                                                                                && f.FechaReserva.Date == FechaReserva.Date
                                                                                                && f.IdRangoReserva == idRango);

                if (reservaMatch != null)
                {
                    reservaMatch.Estado = "CANCELADO";
                    reservaMatch.CantidadPersonas = 0;
                    reservaMatch.FechaModificacion = DateTime.Now;
                    rows = await _reservaContext.SaveChangesAsync();
                }
                else
                {
                    resultado.Exitoso = false;
                    resultado.MensajesError.AppendLine("No se encontró la reserva para cancelar dentro del rango indicado.\n");
                }
            resultado.Exitoso = rows > 0;
            return resultado;
        }

        public async Task<ResultadoOperacion> UpdateReservaFecha(string dni, string fechaAnterior, string fechaActual)
        {   
            var resultado = new ResultadoOperacion();
            int rows = 0;
            DateTime FechaReservaAnterior;
            DateTime FechaReservaActual;

            FechaReservaAnterior = await ConversionDateTime(fechaAnterior);
            FechaReservaActual = await ConversionDateTime(fechaActual);

            var resultadoFecha = await TieneReservaExistenteEnFecha(dni, FechaReservaAnterior);

            if (!resultadoFecha.Exitoso)
            {
                return resultadoFecha;
            }

            var reservaMatch = await _reservaContext.Reservas.FirstOrDefaultAsync(f => f.Dni == dni
                                                                                                && f.FechaReserva.Date == FechaReservaAnterior.Date);
            var espacioDisponibleResponse = await EspacioDisponible(FechaReservaActual, reservaMatch.IdRangoReserva);

            if (espacioDisponibleResponse < reservaMatch.CantidadPersonas)
                resultado.MensajesError.AppendLine($"La cantidad de personas no puede ser mayor al espacio disponible. Espacio disponible: {espacioDisponibleResponse}.\n");


            if(resultado.MensajesError.Length == 0)
            {
                reservaMatch.FechaReserva = FechaReservaActual;
                reservaMatch.FechaModificacion = DateTime.Now;
                reservaMatch.Estado = "CONFIRMADO";
                rows = await _reservaContext.SaveChangesAsync();
            }
            resultado.Exitoso = rows > 0;
            return resultado;

        }

        public async Task<ResultadoOperacion> UpdateReservaRango(string dni, string fecha, int rango)
        {
            var resultado = new ResultadoOperacion();
            int rows = 0;
            DateTime FechaReserva;

            FechaReserva = await ConversionDateTime(fecha);
 
            var resultadoFecha = await TieneReservaExistenteEnFecha(dni, FechaReserva);

            if (!resultadoFecha.Exitoso)
            {
                return resultadoFecha;
            }

            var reservaMatch = await _reservaContext.Reservas.FirstOrDefaultAsync(f => f.Dni == dni
                                                                                                && f.FechaReserva.Date == FechaReserva.Date);
            var espacioDisponibleResponse = await EspacioDisponible(FechaReserva, rango);

            if (espacioDisponibleResponse < reservaMatch.CantidadPersonas)
                resultado.MensajesError.AppendLine($"La cantidad de personas no puede ser mayor al espacio disponible. Espacio disponible: {espacioDisponibleResponse}.\n");

            if (resultado.MensajesError.Length == 0)
            {
                reservaMatch.IdRangoReserva = rango;
                reservaMatch.FechaModificacion = DateTime.Now;
                reservaMatch.Estado = "CONFIRMADO";
                rows = await _reservaContext.SaveChangesAsync();
            }
            resultado.Exitoso = rows > 0;
            return resultado;
        }

        public async Task<ResultadoOperacion> UpdateReservaCantidadPersonas(string dni, string fechaReserva, int cantidadPersonas)
        {
            var resultado = new ResultadoOperacion();
            int rows = 0;
            DateTime FechaReserva;

            FechaReserva = await ConversionDateTime(fechaReserva);

            var resultadoFecha = await TieneReservaExistenteEnFecha(dni, FechaReserva);

            if (!resultadoFecha.Exitoso)
            {
                return resultadoFecha;
            }

            var reservaMatch = await _reservaContext.Reservas.FirstOrDefaultAsync(f => f.Dni == dni && f.FechaReserva.Date == FechaReserva.Date);

            var espacioDisponibleActual = await EspacioDisponible(FechaReserva, reservaMatch.IdRangoReserva);
            espacioDisponibleActual += reservaMatch.CantidadPersonas;
            if (espacioDisponibleActual < cantidadPersonas)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine($"No hay suficiente espacio disponible en el rango actual para la nueva cantidad de personas. Espacio disponible: {espacioDisponibleActual}.\n");
                return resultado;
            }

            if (resultado.MensajesError.Length == 0)
            {
                reservaMatch.CantidadPersonas = cantidadPersonas;
                reservaMatch.FechaModificacion = DateTime.Now;
                reservaMatch.Estado = "CONFIRMADO";
                rows = await _reservaContext.SaveChangesAsync();
            }

            resultado.Exitoso = rows > 0;
            return resultado;
        }
        /*public async Task<bool> UpdateReserva( string dni, string fechaAnterior, string fechaActual, int rango, int cantidadPersonas)
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
        */
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
        public async Task<DateTime> ConversionDateTime(string fecha)
        {
            DateTime fechaConvertida;
            if(fecha.IsNullOrEmpty())
            {
                fecha = "05/05/1999";
            }
            fechaConvertida = DateTime.ParseExact(fecha, "dd/MM/yyyy", null);

            return fechaConvertida;
        }

        public async Task<ResultadoOperacion> ComprobacionGeneral(ReservaDTO reserva)
        {
            var resultado = new ResultadoOperacion();

            if (reserva.NombrePersona.IsNullOrEmpty())
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El nombre no puede estar vacío.");
            }

            if (reserva.ApellidoPersona.IsNullOrEmpty())
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El apellido no puede estar vacío.");
            }

            if (reserva.Dni.IsNullOrEmpty())
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El DNI no puede estar vacío.");
            }

            if (reserva.Mail.IsNullOrEmpty())
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El correo electrónico no puede estar vacío.");
            }

            if (reserva.Celular.IsNullOrEmpty())
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El número de celular no puede estar vacío.");
            }

            if(reserva.IdRangoReserva == 0)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El número de idRango no puede estar vacío.");
            }
            var resultadoIdRango = ComprobacionIdRango(reserva.IdRangoReserva, reserva.CantidadPersonas);
            if (!resultadoIdRango.Exitoso)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine(resultadoIdRango.MensajesError.ToString());
            }

            if (reserva.FechaReserva.IsNullOrEmpty())
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("La fecha de reserva no puede estar vacía.");
            }

            return resultado;
        }

        public ResultadoOperacion ComprobacionIdRango(int rango, int cantPersonas)
        {
            var resultado = new ResultadoOperacion();

            if (rango <= 0 || rango >= 5)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El ID de rango debe estar entre 1 y 4.\n");
            }

            if (cantPersonas <= 0 || cantPersonas > 100)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("La cantidad de personas debe estar entre 1 y 100.\n");
            }

            return resultado;
        }

        public async Task<ResultadoOperacion> ComprobacionSobreFecha(string fechaIngresada, DateTime fechaAhora)
        {
            var resultado = new ResultadoOperacion();

            if (!DateTime.TryParseExact(fechaIngresada, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime fechaReserva))
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El formato de fecha ingresado no es válido, debe ser el siguiente: dd/MM/yyyy o tambien puede ser que necesite agregar un 0 delante de los numeros de 1 cifra.");
                return resultado;
            }

            fechaAhora = fechaAhora.Date;

            if (fechaReserva < fechaAhora)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("La fecha de reserva no puede ser anterior a la fecha actual.");
            }

            TimeSpan diferenciaDeSemana = fechaReserva - fechaAhora;
            if (diferenciaDeSemana.TotalDays > 7)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("La anticipación de la reserva como maximo es de 7 días.");
            }

            return resultado;
        }

        public async Task<ResultadoOperacion> TieneReservaExistenteEnFecha(string dni, DateTime fechaReserva)
        {
            var resultado = new ResultadoOperacion();

            if (string.IsNullOrEmpty(dni))
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("El DNI no puede estar vacío.\n");
                return resultado;
            }


            var dniExistente = await _reservaContext.Reservas
                                                             .AnyAsync(r => r.Dni == dni);

            if (!dniExistente)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine($"No se encontró un registro confirmado para el DNI {dni}.\n");
                return resultado;
            }

            var reservaExistente = await _reservaContext.Reservas
                .FirstOrDefaultAsync(r => r.Dni == dni
                                            && r.FechaReserva.Date == fechaReserva.Date
                                            && r.Estado.ToUpper() == "CONFIRMADO");

            if (reservaExistente == null)
            {
                resultado.Exitoso = false;
                resultado.MensajesError.AppendLine("No se encontró una reserva confirmada para el DNI y la fecha proporcionados.\n");
            }

            return resultado;
        }

        public async Task<int> EspacioDisponible(DateTime fechaReserva, int idRango)
        {
            var reservasConfirmadas = await _reservaContext.Reservas
                .Where(r => r.FechaReserva.Date == fechaReserva.Date
                            && r.IdRangoReserva == idRango
                            && r.Estado.ToUpper() == "CONFIRMADO")
                .ToListAsync(); 

            int cantidadPersonas = reservasConfirmadas.Sum(r => r.CantidadPersonas);

            int cupoMaximo = await _reservaContext.RangoReservas
                .Where(r => r.IdRangoReserva == idRango)
                .Select(r => r.Cupo)
                .FirstOrDefaultAsync(); 

            int espacioDisponible = cupoMaximo - cantidadPersonas;

            return espacioDisponible;
        }
    }
}
