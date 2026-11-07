using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Helpers;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class RutaProblemaService : IRutaProblemaService
    {
        private const int MaxAnalisis = 1000;
        private const int MaxAvance = 500;
        private const int MaxResultados = 1500;

        private readonly AppDbContext _context;

        public RutaProblemaService(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------- consulta

        public async Task<RutaProblemaVm> ObtenerAsync(int problemaId, int? usuarioId)
        {
            var problema = await _context.problemas
                .AsNoTracking()
                .Where(p => p.Id == problemaId)
                .Select(p => new { p.UsuarioId, p.Causas, p.Consecuencias, TotalSoluciones = p.Soluciones.Count() })
                .FirstOrDefaultAsync();

            var vm = new RutaProblemaVm
            {
                ProblemaId = problemaId,
                Autenticado = usuarioId.HasValue,
                EsDueno = usuarioId.HasValue && problema?.UsuarioId == usuarioId,
                Causas = problema?.Causas,
                Consecuencias = problema?.Consecuencias
            };

            // Solución elegida (si existe) con su equipo y avances
            var seleccionada = await _context.soluciones
                .AsNoTracking()
                .Where(s => s.ProblemaId == problemaId && s.Seleccionada)
                .Select(s => new
                {
                    s.Id,
                    s.UsuarioId,
                    s.Descripcion,
                    s.Estado,
                    s.Fecha,
                    s.Impacto,
                    s.Costo,
                    s.TiempoEstimado,
                    s.Resultados,
                    s.FechaResultados,
                    Autor = s.Usuario!.Nombre + " " + s.Usuario.Apellido,
                    TotalLikes = s.Likes.Count()
                })
                .FirstOrDefaultAsync();

            if (seleccionada != null)
            {
                vm.Seleccionada = new SolucionDto
                {
                    Id = seleccionada.Id,
                    Problema_Id = problemaId,
                    UsuarioId = seleccionada.UsuarioId,
                    Descripcion = seleccionada.Descripcion,
                    Estado = seleccionada.Estado,
                    Fecha_Solucion = seleccionada.Fecha,
                    UsuarioNombre = seleccionada.Autor,
                    Impacto = seleccionada.Impacto,
                    Costo = seleccionada.Costo,
                    TiempoEstimado = seleccionada.TiempoEstimado,
                    Seleccionada = true,
                    TotalLikes = seleccionada.TotalLikes
                };
                vm.Resultados = seleccionada.Resultados;
                vm.FechaResultados = seleccionada.FechaResultados;

                // Equipo: primero el líder (autor de la solución) y luego quienes se unieron
                vm.Equipo.Add(new MiembroDto
                {
                    UsuarioId = seleccionada.UsuarioId,
                    Nombre = seleccionada.Autor,
                    EsLider = true,
                    Fecha = seleccionada.Fecha
                });

                var colaboradores = await _context.Colaboradores
                    .AsNoTracking()
                    .Where(c => c.SolucionId == seleccionada.Id)
                    .OrderBy(c => c.FechaUnion)
                    .Select(c => new MiembroDto
                    {
                        UsuarioId = c.UsuarioId,
                        Nombre = c.Usuario!.Nombre + " " + c.Usuario.Apellido,
                        EsLider = false,
                        Fecha = c.FechaUnion
                    })
                    .ToListAsync();
                vm.Equipo.AddRange(colaboradores);

                vm.Avances = await _context.Avances
                    .AsNoTracking()
                    .Where(a => a.SolucionId == seleccionada.Id)
                    .OrderByDescending(a => a.Fecha)
                    .Select(a => new AvanceDto
                    {
                        Id = a.Id,
                        UsuarioId = a.UsuarioId,
                        Nombre = a.Usuario!.Nombre + " " + a.Usuario.Apellido,
                        Contenido = a.Contenido,
                        Porcentaje = a.Porcentaje,
                        Fecha = a.Fecha
                    })
                    .ToListAsync();

                // El progreso es el último porcentaje reportado
                vm.Progreso = vm.Avances.Where(a => a.Porcentaje.HasValue)
                                        .Select(a => a.Porcentaje!.Value)
                                        .FirstOrDefault();
                if (vm.Resuelto) vm.Progreso = 100;

                if (usuarioId.HasValue)
                {
                    vm.EsLider = seleccionada.UsuarioId == usuarioId.Value;
                    vm.EsColaborador = colaboradores.Any(c => c.UsuarioId == usuarioId.Value);
                }
            }

            vm.Etapa = RutaProblemaHelper.CalcularEtapa(
                vm.CausasDocumentadas,
                problema?.TotalSoluciones ?? 0,
                seleccionada != null,
                vm.Avances.Count,
                vm.Resuelto);

            return vm;
        }

        // ---------------------------------------------------------------- paso 2

        public async Task<string?> GuardarAnalisisAsync(int problemaId, string? causas, string? consecuencias, int usuarioId)
        {
            var problema = await _context.problemas.FindAsync(problemaId);
            if (problema == null) return "El problema no existe.";
            if (problema.UsuarioId != usuarioId) return "Solo quien publicó el problema puede documentar sus causas y consecuencias.";

            causas = Limpiar(causas);
            consecuencias = Limpiar(consecuencias);

            if ((causas?.Length ?? 0) > MaxAnalisis || (consecuencias?.Length ?? 0) > MaxAnalisis)
                return $"Cada campo admite hasta {MaxAnalisis} caracteres.";

            problema.Causas = causas;
            problema.Consecuencias = consecuencias;
            await _context.SaveChangesAsync();
            return null;
        }

        // ---------------------------------------------------------------- paso 4 -> 5

        public async Task<string?> SeleccionarSolucionAsync(int problemaId, int solucionId, int usuarioId)
        {
            var problema = await _context.problemas.FindAsync(problemaId);
            if (problema == null) return "El problema no existe.";
            if (problema.UsuarioId != usuarioId) return "Solo quien publicó el problema puede elegir la solución.";

            var soluciones = await _context.soluciones
                .Where(s => s.ProblemaId == problemaId)
                .ToListAsync();

            var elegida = soluciones.FirstOrDefault(s => s.Id == solucionId);
            if (elegida == null) return "La solución no pertenece a este problema.";

            if (soluciones.Any(s => s.FechaResultados != null))
                return "Este problema ya está resuelto.";

            var actual = soluciones.FirstOrDefault(s => s.Seleccionada);
            if (actual != null && actual.Id == elegida.Id) return null;

            // Cambiar de solución solo es posible mientras no se hayan registrado avances
            if (actual != null && await _context.Avances.AnyAsync(a => a.SolucionId == actual.Id))
                return "Ya hay avances registrados, no se puede cambiar la solución elegida.";

            foreach (var s in soluciones)
            {
                s.Seleccionada = s.Id == elegida.Id;
                s.Estado = s.Id == elegida.Id ? "Seleccionada" : "Propuesta";
            }

            await _context.SaveChangesAsync();
            return null;
        }

        // ---------------------------------------------------------------- paso 5

        public async Task<string?> UnirseAsync(int solucionId, int usuarioId)
        {
            var solucion = await _context.soluciones.AsNoTracking().FirstOrDefaultAsync(s => s.Id == solucionId);
            if (solucion == null || !solucion.Seleccionada) return "Solo puedes unirte al equipo de la solución elegida.";
            if (solucion.FechaResultados != null) return "Este problema ya está resuelto.";
            if (solucion.UsuarioId == usuarioId) return "Ya eres el líder de este equipo.";

            if (await _context.Colaboradores.AnyAsync(c => c.SolucionId == solucionId && c.UsuarioId == usuarioId))
                return null; // ya estaba en el equipo

            await _context.Colaboradores.AddAsync(new Colaborador
            {
                SolucionId = solucionId,
                UsuarioId = usuarioId,
                FechaUnion = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> SalirAsync(int solucionId, int usuarioId)
        {
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.SolucionId == solucionId && c.UsuarioId == usuarioId);
            if (colaborador == null) return null;

            _context.Colaboradores.Remove(colaborador);
            await _context.SaveChangesAsync();
            return null;
        }

        // ---------------------------------------------------------------- paso 6

        public async Task<string?> AgregarAvanceAsync(int solucionId, string? contenido, int? porcentaje, int usuarioId)
        {
            var solucion = await _context.soluciones
                .AsNoTracking()
                .Include(s => s.problema)
                .FirstOrDefaultAsync(s => s.Id == solucionId);

            if (solucion == null || !solucion.Seleccionada) return "Los avances se registran sobre la solución elegida.";
            if (solucion.FechaResultados != null) return "Este problema ya está resuelto.";
            if (!await PuedeGestionarAsync(solucion, usuarioId))
                return "Solo el equipo de la solución o el creador del problema pueden registrar avances.";

            contenido = Limpiar(contenido);
            if (contenido == null) return "Describe el avance que lograron.";
            if (contenido.Length > MaxAvance) return $"El avance admite hasta {MaxAvance} caracteres.";
            if (porcentaje is < 0 or > 100) return "El porcentaje debe estar entre 0 y 100.";

            await _context.Avances.AddAsync(new Avance
            {
                SolucionId = solucionId,
                UsuarioId = usuarioId,
                Contenido = contenido,
                Porcentaje = porcentaje,
                Fecha = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> EliminarAvanceAsync(int avanceId, int usuarioId)
        {
            var avance = await _context.Avances.FindAsync(avanceId);
            if (avance == null) return null;
            if (avance.UsuarioId != usuarioId) return "Solo quien registró el avance puede eliminarlo.";

            var resuelto = await _context.soluciones
                .AnyAsync(s => s.Id == avance.SolucionId && s.FechaResultados != null);
            if (resuelto) return "Este problema ya está resuelto.";

            _context.Avances.Remove(avance);
            await _context.SaveChangesAsync();
            return null;
        }

        // ---------------------------------------------------------------- paso 7

        public async Task<string?> DocumentarResultadosAsync(int solucionId, string? resultados, int usuarioId)
        {
            var solucion = await _context.soluciones
                .Include(s => s.problema)
                .FirstOrDefaultAsync(s => s.Id == solucionId);

            if (solucion == null || !solucion.Seleccionada) return "Los resultados se documentan sobre la solución elegida.";
            if (solucion.FechaResultados != null) return "Los resultados ya fueron documentados.";
            if (!await PuedeGestionarAsync(solucion, usuarioId))
                return "Solo el equipo de la solución o el creador del problema pueden documentar los resultados.";

            resultados = Limpiar(resultados);
            if (resultados == null) return "Describe los resultados obtenidos.";
            if (resultados.Length > MaxResultados) return $"Los resultados admiten hasta {MaxResultados} caracteres.";

            solucion.Resultados = resultados;
            solucion.FechaResultados = DateTime.Now;
            solucion.Estado = "Implementada";
            await _context.SaveChangesAsync();
            return null;
        }

        // ---------------------------------------------------------------- utilidades

        // Puede gestionar: el creador del problema, el líder o un colaborador del equipo
        private async Task<bool> PuedeGestionarAsync(Solucion solucion, int usuarioId)
        {
            if (solucion.UsuarioId == usuarioId) return true;
            if (solucion.problema?.UsuarioId == usuarioId) return true;

            return await _context.Colaboradores
                .AnyAsync(c => c.SolucionId == solucion.Id && c.UsuarioId == usuarioId);
        }

        private static string? Limpiar(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }
    }
}
