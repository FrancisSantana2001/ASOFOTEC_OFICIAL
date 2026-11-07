namespace ASOFOTEC.Helpers
{
    public static class RolesSistema
    {
        public const string Usuario = "Usuario";
        public const string Admin = "Admin";

        // Administrador único de la plataforma (funciones especiales de gestión y moderación)
        public const string Dba = "DBA";

        public static readonly string[] Todos = { Usuario, Admin, Dba };

        // Roles que el DBA puede asignar desde el panel (el rol DBA nunca se asigna desde la interfaz)
        public static readonly string[] Asignables = { Usuario, Admin };
    }
}
