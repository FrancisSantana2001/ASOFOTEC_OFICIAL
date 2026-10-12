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

        //Obtener todos los problemas
        public async Task<List<ProblemaDto>> ObtenerTodosAsync()
        {
            return await _Context.problemas
                .OrderByDescending(p => p.FechaCreación)
                .Select(p => new ProblemaDto
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    Descripcion = p.Descripcion,
                    Categoria = p.Categoria,
                    ImageUrl = p.ImgeUrl,
                    Fecha_Creacion = p.FechaCreación
                })
                .ToListAsync();
        }
        //Obtener un problema por su ID
        public async Task<ProblemaDto?> ObtenerPorIdAsync(int id)
        {
            var problema = await _Context.problemas
                .Where(p => p.Id == id)
                .Select(p=> new ProblemaDto
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    Descripcion = p.Descripcion,
                    Categoria = p.Categoria,
                    Fecha_Creacion = p.FechaCreación
                })
                .FirstOrDefaultAsync();
            return problema;
        }

        //Crear un nuevo problema
        public async Task<bool> CreateAsync(ProblemaCreateDto ProblemaCreateDto)
        { 
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
                ImgeUrl = RutaImagen,
                FechaCreación = DateTime.Now
            };

            if (ProblemaCreateDto == null)
            {
                return false;
            }

            await _Context.problemas.AddAsync(nuevoProblema);
            await _Context.SaveChangesAsync();
            return true;

        }

        //Eliminar un problema
        public async Task<bool> ElimiarAsync(int id)
        { 
            var problema = await _Context.problemas.FindAsync(id);
            if ( problema == null)
            {
                return false;
            }
            _Context.problemas.Remove(problema);
            await _Context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProblemaDto>> ObtenerDestacadosAsync(int cantidad = 3)
        {
            return await _Context.problemas
                .OrderByDescending(p => p.FechaCreación)
                .Take(cantidad)
                .Select(p => new ProblemaDto
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    Descripcion = p.Descripcion,
                    Categoria = p.Categoria,
                    ImageUrl = p.ImgeUrl,
                    Fecha_Creacion = p.FechaCreación
                })
                .ToListAsync();
        }

        public async Task<int> ObtenerTotalProblemasAsync()
        {
            return await _Context.problemas.CountAsync();
        }
    }
}
