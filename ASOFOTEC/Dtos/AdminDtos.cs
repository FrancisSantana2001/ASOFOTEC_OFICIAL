namespace ASOFOTEC.Dtos
{
    public class MetricasDbaDto
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosInactivos { get; set; }
        public int NuevosUltimos7Dias { get; set; }
        public int NuevosUltimos30Dias { get; set; }

        public int Masculino { get; set; }
        public int Femenino { get; set; }
        public int SinDato { get; set; }

        public int TotalProblemas { get; set; }
        public int TotalSoluciones { get; set; }
        public int TotalPublicaciones { get; set; }
        public int TotalComentarios { get; set; }
        public int TotalProyectosEnMarcha { get; set; }
        public int TotalResueltos { get; set; }
    }

    // Fila de la tabla de usuarios del panel DBA. No incluye teléfono ni fecha de nacimiento
    public class UsuarioAdminDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario";
        public bool Activo { get; set; }
        public DateTime FechaIngreso { get; set; }
        public int? Edad { get; set; }
        public int Problemas { get; set; }
        public int Soluciones { get; set; }
        public int Publicaciones { get; set; }
        public int Comentarios { get; set; }
        public string? FotoPerfilUrl { get; set; }
    }
}
