using ASOFOTEC.Data;
using ASOFOTEC.Dtos;
using ASOFOTEC.Models;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Services.Implementations
{
    public class ProblemaService : IProblemaService
    {
        private readonly AppDbContext _Context;
        public ProblemaService(AppDbContext dbContext)
        {
            _Context = dbContext;
        }

        // Proyección común: datos del problema + lo necesario para saber en qué paso de la ruta va
        private static IQueryable<ProblemaDto> Proyectar(IQueryable<Problema> consulta)
        {
            return consulta.Select(p => new ProblemaDto
            {
                Id = p.Id,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion,
                Categoria = p.Categoria,
                ImageUrl = p.ImgeUrl,
                Fecha_Creacion = p.FechaCreación,
                UsuarioId = p.UsuarioId,
                UsuarioNombre = p.Usuario != null ? p.Usuario.Nombre + " " + p.Usuario.Apellido : null,
                CausasDocumentadas = (p.Causas ?? "") != "" || (p.Consecuencias ?? "") != "",
                TotalSoluciones = p.Soluciones.Count(),
                Seleccionada = p.Soluciones.Any(s => s.Seleccionada),
                TotalAvances = p.Soluciones.Where(s => s.Seleccionada).SelectMany(s => s.Avances).Count(),
                Resuelto = p.Soluciones.Any(s => s.Seleccionada && s.FechaResultados != null)
            });
        }

        //Obtener todos los problemas
        public async Task<List<ProblemaDto>> ObtenerTodosAsync(string? busqueda = null)
        {
            var consulta = _Context.problemas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                consulta = consulta.Where(p =>
                    (p.Titulo ?? "").Contains(texto) ||
                    (p.Descripcion ?? "").Contains(texto) ||
                    (p.Categoria ?? "").Contains(texto));
            }

            return await Proyectar(consulta.OrderByDescending(p => p.FechaCreación))
                .ToListAsync();
        }

        public async Task<List<ProblemaDto>> ObtenerProyectosAsync()
        {
            return await Proyectar(_Context.problemas
                    .Where(p => p.Soluciones.Any(s => s.Seleccionada))
                    .OrderByDescending(p => p.FechaCreación))
                .ToListAsync();
        }

        public async Task<int> ObtenerTotalProyectosAsync()
        {
            return await _Context.problemas
                .CountAsync(p => p.Soluciones.Any(s => s.Seleccionada && s.FechaResultados == null));
        }

        public async Task<int> ObtenerTotalResueltosAsync()
        {
            return await _Context.problemas
                .CountAsync(p => p.Soluciones.Any(s => s.Seleccionada && s.FechaResultados != null));
        }

        //Obtener un problema por su ID
        public async Task<ProblemaDto?> ObtenerPorIdAsync(int id)
        {
            return await Proyectar(_Context.problemas.Where(p => p.Id == id))
                .FirstOrDefaultAsync();
        }

        //Crear un nuevo problema
        public async Task<bool> CreateAsync(ProblemaCreateDto ProblemaCreateDto, int usuarioId)
        { 
            if (ProblemaCreateDto == null)
            {
                return false;
            }

            string? RutaImagen = null;
            if(ProblemaCreateDto.Imagen != null)
            {
                var ArchName = Guid.NewGuid().ToString() + Path.GetExtension(ProblemaCreateDto.Imagen.FileName);
                var ruta = Path.Combine("wwwroot/img/ImgProblemas", ArchName);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await ProblemaCreateDto.Imagen.CopyToAsync(stream);
                }

                RutaImagen = "/img/ImgProblemas/" + ArchName;
            }

            
            var nuevoProblema = new Problema
            {
                Titulo = ProblemaCreateDto.Titulo,
                Descripcion = ProblemaCreateDto.Descripcion,
                Categoria = ProblemaCreateDto.Categoria,
                Causas = string.IsNullOrWhiteSpace(ProblemaCreateDto.Causas) ? null : ProblemaCreateDto.Causas.Trim(),
                Consecuencias = string.IsNullOrWhiteSpace(ProblemaCreateDto.Consecuencias) ? null : ProblemaCreateDto.Consecuencias.Trim(),
                ImgeUrl = RutaImagen,
                FechaCreación = DateTime.Now,
                UsuarioId = usuarioId
            };

            await _Context.problemas.AddAsync(nuevoProblema);
            await _Context.SaveChangesAsync();
            return true;

        }

        //Eliminar un problema
        public async Task<bool> ElimiarAsync(int id, int usuarioId, bool esModerador = false)
        { 
            var problema = await _Context.problemas.FindAsync(id);

            // Solo puede eliminarlo quien lo publicó (o el DBA, como moderación)
            if (problema == null || (problema.UsuarioId != usuarioId && !esModerador))
            {
                return false;
            }
            _Context.problemas.Remove(problema);
            await _Context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProblemaDto>> ObtenerDestacadosAsync(int cantidad = 3)
        {
            // Destacados: los que más soluciones han recibido (y, a igualdad, los más recientes)
            return await Proyectar(_Context.problemas
                    .OrderByDescending(p => p.Soluciones.Count())
                    .ThenByDescending(p => p.FechaCreación)
                    .Take(cantidad))
                .ToListAsync();
        }

        public async Task<int> ObtenerTotalProblemasAsync()
        {
            return await _Context.problemas.CountAsync();
        }
    }
}
