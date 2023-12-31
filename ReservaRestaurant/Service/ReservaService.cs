﻿using ReservaRestaurant.Domain.DTO;
using ReservaRestaurant.Domain.Entities;
using ReservaRestaurant.Reportes;
using ReservaRestaurant.Repository.Interfaces;
using ReservaRestaurant.Service.Interfaces;

namespace ReservaRestaurant.Service
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepository;

        public ReservaService(IReservaRepository reservaRepository)
        {
            _reservaRepository = reservaRepository;
        }

        public async Task<ResultadoOperacion> AddReservaServiceAsync(ReservaDTO reserva)
        {
            var result = await _reservaRepository.AddReserva(reserva);
            return result;
        }
        
        public async Task<ResultadoOperacion> CancelarReservaServiceAsync(string dni, string fechaReserva, int idRango)
        {
            var result = await _reservaRepository.CancelarReserva(dni, fechaReserva, idRango);
            return result;
        }

        /*public async Task<bool> UpdateReservaServiceAsync(string dni, string fechaAnterior, string fechaActual, int rango, int cantidadPersonas)
        {
            var result = await _reservaRepository.UpdateReserva(dni, fechaAnterior, fechaActual , rango, cantidadPersonas);
            return result;
        }
        */

        public async Task<ResultadoOperacion> UpdateReservaFechaServiceAsync(string dni, string fechaAnterior, string fechaActual)
        {
            var result = await _reservaRepository.UpdateReservaFecha(dni, fechaAnterior, fechaActual);
            return result;
        }

        public async Task<ResultadoOperacion> UpdateReservaRangoServiceAsync(string dni, string fechaAnterior, int rango)
        {
            var result = await _reservaRepository.UpdateReservaRango(dni, fechaAnterior, rango);
            return result;
        }
        public async Task<ResultadoOperacion> UpdateReservaCantidadPersonasServiceAsync(string dni, string fechaAnterior, int cantidadPersonas)
        {
            var result = await _reservaRepository.UpdateReservaCantidadPersonas(dni, fechaAnterior, cantidadPersonas);
            return result;
        }
        public async Task<List<CalendarioDTO>> GetListarTurnosDisponiblesServiceAsync()
        {
            var result = await _reservaRepository.GetListarTurnosDisponibles();

            return result;
        }

        public async Task<List<CalendarioDTO>> GetListarSinTurnosDisponiblesServiceAsync()
        {
            var result = await _reservaRepository.GetListarSinTurnosDisponibles();

            return result;
        }

        public async Task<List<CalendarioDTO>> GetListarTurnosCanceladosServiceAsync()
        {
            var result = await _reservaRepository.GetListarTurnosCancelados();

            return result;
        }

        public async Task<List<CalendarioDTO>> GetListarTurnosConfirmadosServiceAsync()
        {
            var result = await _reservaRepository.GetListarTurnosConfirmados();

            return result;
        }
    }
}
