using MR.Abstracciones.AccesoADatos.Finanzas;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Finanzas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.LogicaNegocio.Finanzas
{
    public class FinanzasLN : IFinanzasLN
    {

        private readonly IFinanzasAD _finanzasAD;

        public FinanzasLN(IFinanzasAD finanzasAD)
        {
            _finanzasAD = finanzasAD;
        }

        public async Task<IEnumerable<MovimientoFinancieroDto>> ObtenerMovimientosAsync(
            string? textoBusqueda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoFiltro,
            string? origenFiltro)
        {
            return await _finanzasAD.ObtenerMovimientosAsync(
                textoBusqueda,
                desde,
                hasta,
                tipoFiltro,
                origenFiltro
            );
        }

        public async Task<ResumenContabilidadDto> ObtenerResumenAsync(
            DateTime? desde,
            DateTime? hasta,
            string? tipoFiltro,
            string? origenFiltro)
        {
            return await _finanzasAD.ObtenerResumenAsync(
                desde,
                hasta,
                tipoFiltro,
                origenFiltro
            );
        }

        public async Task<int> RegistrarMovimientoAsync(MovimientoFinancieroDto movimiento)
        {
            if (movimiento == null)
                throw new Exception("El movimiento no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(movimiento.Tipo))
                throw new Exception("El tipo del movimiento es obligatorio.");

            if (movimiento.Tipo != "Ingreso" && movimiento.Tipo != "Egreso")
                throw new Exception("El tipo del movimiento debe ser 'Ingreso' o 'Egreso'.");

            if (movimiento.Monto <= 0)
                throw new Exception("El monto debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(movimiento.Categoria))
                throw new Exception("La categoría es obligatoria.");

            if (movimiento.Fecha == default)
                movimiento.Fecha = DateTime.Now;

            if (movimiento.IdUsuario <= 0)
                throw new Exception("El usuario que registra el movimiento es obligatorio.");

            if (string.IsNullOrWhiteSpace(movimiento.Origen))
                movimiento.Origen = "Manual";

            return await _finanzasAD.RegistrarMovimientoAsync(movimiento);
        }

        public async Task<IEnumerable<CierreContableDto>> ObtenerCierresAsync()
        {
            return await _finanzasAD.ObtenerCierresAsync();
        }

        public async Task<int> RegistrarCierreAsync(CierreContableDto cierre)
        {
            if (cierre == null)
                throw new Exception("El cierre no puede ser nulo.");

            if (cierre.FechaInicio == default)
                throw new Exception("La fecha inicial del cierre es obligatoria.");

            if (cierre.FechaFin == default)
                throw new Exception("La fecha final del cierre es obligatoria.");

            if (cierre.FechaFin < cierre.FechaInicio)
                throw new Exception("La fecha final no puede ser menor a la fecha inicial.");

            if (string.IsNullOrWhiteSpace(cierre.Tipo))
                cierre.Tipo = "Personalizado";

            if (cierre.FechaRegistro == default)
                cierre.FechaRegistro = DateTime.Now;

            if (cierre.IdUsuario <= 0)
                throw new Exception("El usuario que realiza el cierre es obligatorio.");

            var resumen = await _finanzasAD.ObtenerResumenAsync(
                cierre.FechaInicio,
                cierre.FechaFin,
                null,
                null
            );

            cierre.TotalIngresos = resumen.TotalIngresos;
            cierre.TotalEgresos = resumen.TotalEgresos;
            cierre.BalanceTotal = resumen.BalanceTotal;

            return await _finanzasAD.RegistrarCierreAsync(cierre);
        }

        public async Task<int> RegistrarIngresoAutomaticoAsync(
            decimal monto,
            int idUsuario,
            string categoria,
            string descripcion,
            string origen,
            int? idReferencia)
        {
            if (monto <= 0)
                throw new Exception("El monto del ingreso debe ser mayor que cero.");

            if (idUsuario <= 0)
                throw new Exception("El usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(categoria))
                throw new Exception("La categoría es obligatoria.");

            if (string.IsNullOrWhiteSpace(origen))
                throw new Exception("El origen es obligatorio.");

            var movimiento = new MovimientoFinancieroDto
            {
                Tipo = "Ingreso",
                Monto = monto,
                Fecha = DateTime.Now,
                Categoria = categoria,
                Descripcion = descripcion,
                IdUsuario = idUsuario,
                Origen = origen,
                IdReferencia = idReferencia
            };

            return await _finanzasAD.RegistrarMovimientoAsync(movimiento);
        }

        public async Task<int> RegistrarEgresoAutomaticoAsync(
            decimal monto,
            int idUsuario,
            string categoria,
            string descripcion,
            string origen,
            int? idReferencia)
        {
            if (monto <= 0)
                throw new Exception("El monto del egreso debe ser mayor que cero.");

            if (idUsuario <= 0)
                throw new Exception("El usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(categoria))
                throw new Exception("La categoría es obligatoria.");

            if (string.IsNullOrWhiteSpace(origen))
                throw new Exception("El origen es obligatorio.");

            var movimiento = new MovimientoFinancieroDto
            {
                Tipo = "Egreso",
                Monto = monto,
                Fecha = DateTime.Now,
                Categoria = categoria,
                Descripcion = descripcion,
                IdUsuario = idUsuario,
                Origen = origen,
                IdReferencia = idReferencia
            };

            return await _finanzasAD.RegistrarMovimientoAsync(movimiento);
        }

    }
}
