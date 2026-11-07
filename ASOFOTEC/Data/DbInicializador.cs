using ASOFOTEC.Helpers;
using ASOFOTEC.Models;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Data
{
    // Al arrancar: garantiza que existan los roles y crea (una sola vez) al administrador DBA
    // a partir de la configuración "Dba" (Correo, Password, Nombre, Apellido).
    public static class DbInicializador
    {
        public static async Task AsegurarRolesYDbaAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInicializador");

            try
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                // 1. Roles del sistema
                foreach (var nombre in RolesSistema.Todos)
                {
                    if (!await db.Roles.AnyAsync(r => r.Nombre == nombre))
                    {
                        db.Roles.Add(new Roles { Nombre = nombre });
                    }
                }
                await db.SaveChangesAsync();

                // 2. DBA único: si ya existe uno, no se hace nada
                var rolDba = await db.Roles.FirstAsync(r => r.Nombre == RolesSistema.Dba);
                if (await db.RolesUser.AnyAsync(ur => ur.Rol_Id == rolDba.Id))
                {
                    return;
                }

                var correo = config["Dba:Correo"]?.Trim();
                if (string.IsNullOrEmpty(correo))
                {
                    logger.LogWarning("No hay administrador DBA. Configura 'Dba:Correo' (y 'Dba:Password' si la cuenta no existe) para crearlo.");
                    return;
                }

                var usuario = await db.usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
                if (usuario == null)
                {
                    var password = config["Dba:Password"];
                    if (string.IsNullOrEmpty(password) || password.Length < 8)
                    {
                        logger.LogWarning("No se creó el DBA: 'Dba:Password' debe tener al menos 8 caracteres.");
                        return;
                    }

                    usuario = new Usuario
                    {
                        Nombre = config["Dba:Nombre"] ?? "Administrador",
                        Apellido = config["Dba:Apellido"] ?? "DBA",
                        Correo = correo,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        Fecha_Ingreso = DateTime.Now,
                        Activo = true
                    };
                    db.usuarios.Add(usuario);
                    await db.SaveChangesAsync();
                }

                // El DBA tiene un único rol: se reemplazan los anteriores
                usuario.Activo = true;
                db.RolesUser.RemoveRange(db.RolesUser.Where(ur => ur.Usuario_Id == usuario.Id));
                db.RolesUser.Add(new UsuarioRol { Usuario_Id = usuario.Id, Rol_Id = rolDba.Id });
                await db.SaveChangesAsync();

                logger.LogInformation("Administrador DBA configurado para la cuenta {Correo}.", correo);
            }
            catch (Exception ex)
            {
                // Por ejemplo, si todavía no se aplicaron las migraciones: la aplicación debe poder arrancar igual
                logger.LogWarning(ex, "No se pudieron verificar los roles ni el DBA. ¿Ya ejecutaste Update-Database?");
            }
        }
    }
}
