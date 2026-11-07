using ASOFOTEC.Helpers;

namespace ASOFOTEC.Dtos
{
    // Una persona en listas de sugerencias, búsqueda, seguidores, etc.
    public class PersonaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }

        public bool SigoYo { get; set; }      // yo la sigo
        public bool MeSigue { get; set; }     // ella me sigue
        public bool EsAmigo => SigoYo && MeSigue;

        // Cuántas personas que yo sigo también la siguen (conexiones en común)
        public int EnComun { get; set; }
    }

    public class NotificacionDto
    {
        public int Id { get; set; }
        public int ActorId { get; set; }
        public string ActorNombre { get; set; } = string.Empty;
        public string? ActorFoto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? Extracto { get; set; }
        public string Url { get; set; } = "/";
        public bool Leida { get; set; }
        public DateTime Fecha { get; set; }
    }

    public record SeguimientoResultado(bool Siguiendo, int TotalSeguidores);

    // Perfil que ve otra persona: sin correo, teléfono, fecha de nacimiento ni sexo
    public class PerfilPublicoVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string? FotoPortadaUrl { get; set; }
        public DateTime FechaIngreso { get; set; }

        public int TotalPublicaciones { get; set; }
        public int TotalProblemas { get; set; }
        public int TotalSoluciones { get; set; }

        public int Seguidores { get; set; }
        public int Siguiendo { get; set; }

        public bool SigoYo { get; set; }
        public bool MeSigue { get; set; }
        public bool EsAmigo => SigoYo && MeSigue;

        // Bloqueos entre el visitante y este perfil
        public bool BloqueadoPorMi { get; set; }
        public bool MeBloqueo { get; set; }

        // Personas que yo sigo y que también siguen a este usuario
        public List<PersonaDto> EnComun { get; set; } = new();

        public List<FeedItemDto> Actividad { get; set; } = new();
    }

    public class PersonasVm
    {
        public string? Busqueda { get; set; }
        public List<PersonaDto> Resultados { get; set; } = new();
        public List<PersonaDto> Sugerencias { get; set; } = new();
    }

    public class ConexionesVm
    {
        public int PerfilId { get; set; }
        public string PerfilNombre { get; set; } = string.Empty;
        public bool EsSeguidores { get; set; }
        public List<PersonaDto> Personas { get; set; } = new();
    }
}

namespace ASOFOTEC.Dtos
{
    public class NotificacionesVm
    {
        public List<NotificacionDto> Items { get; set; } = new();
        public int Pagina { get; set; } = 1;
        public bool HayMas { get; set; }
    }

    // Datos del botón "Compartir"
    public class CompartirVm
    {
        public string Url { get; set; } = string.Empty;      // dirección completa (https://sitio/...)
        public string Titulo { get; set; } = string.Empty;
        public string Clase { get; set; } = "btn btn-sm btn-outline-success";
    }
}
