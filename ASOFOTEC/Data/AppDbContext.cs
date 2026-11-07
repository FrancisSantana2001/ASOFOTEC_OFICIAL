using ASOFOTEC.Models;
using Microsoft.EntityFrameworkCore;

namespace ASOFOTEC.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> Options) : base(Options)
        {
        }

        public DbSet<Usuario> usuarios => Set<Usuario>();
        public DbSet<Publicacion> publicaciones => Set<Publicacion>();
        public DbSet<Solucion> soluciones => Set<Solucion>();
        public DbSet<Problema> problemas => Set<Problema>();
        public DbSet<UsuarioRol> RolesUser => Set<UsuarioRol>();
        public DbSet<Roles> Roles => Set<Roles>();
        public DbSet<Comentario> Comentarios => Set<Comentario>();
        public DbSet<LikeSolucion> LikeSoluciones => Set<LikeSolucion>();
        public DbSet<LikePublicacion> LikePublicaciones => Set<LikePublicacion>();
        public DbSet<LikeComentario> LikeComentarios => Set<LikeComentario>();
        public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
        public DbSet<Avance> Avances => Set<Avance>();
        public DbSet<Seguimiento> Seguimientos => Set<Seguimiento>();
        public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
        public DbSet<Conversacion> Conversaciones => Set<Conversacion>();
        public DbSet<Mensaje> Mensajes => Set<Mensaje>();
        public DbSet<Bloqueo> Bloqueos => Set<Bloqueo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.Usuario_Id, ur.Rol_Id });

            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.usuario)
                .WithMany()
                .HasForeignKey(r => r.Usuario_Id);

            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Rol)
                .WithMany(r => r.UserRol)
                .HasForeignKey(r => r.Rol_Id);

            // Problema -> Usuario (dueño de la publicación)
            modelBuilder.Entity<Problema>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Problemas)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            // Comentario -> Usuario
            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Comentarios)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comentario>()
            .HasOne(c => c.Solucion)
            .WithMany(s => s.Comentarios)
            .HasForeignKey(c => c.SolucionId)
            .OnDelete(DeleteBehavior.Cascade);

            // Seguimiento: un usuario no puede seguir dos veces a la misma persona
            modelBuilder.Entity<Seguimiento>()
                .HasOne(s => s.Seguidor)
                .WithMany()
                .HasForeignKey(s => s.SeguidorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Seguimiento>()
                .HasOne(s => s.Seguido)
                .WithMany()
                .HasForeignKey(s => s.SeguidoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Seguimiento>()
                .HasIndex(s => new { s.SeguidorId, s.SeguidoId })
                .IsUnique();

            // Notificacion: dos relaciones con Usuario (quien recibe y quien actúa)
            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Usuario)
                .WithMany()
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Actor)
                .WithMany()
                .HasForeignKey(n => n.ActorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notificacion>()
                .HasIndex(n => new { n.UsuarioId, n.Leida, n.Fecha });

            // Conversacion: una sola por pareja de usuarios
            modelBuilder.Entity<Conversacion>()
                .HasOne(c => c.UsuarioA)
                .WithMany()
                .HasForeignKey(c => c.UsuarioAId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Conversacion>()
                .HasOne(c => c.UsuarioB)
                .WithMany()
                .HasForeignKey(c => c.UsuarioBId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Conversacion>()
                .HasIndex(c => new { c.UsuarioAId, c.UsuarioBId })
                .IsUnique();

            // Mensaje: se borran junto con su conversación
            modelBuilder.Entity<Mensaje>()
                .HasOne(m => m.Conversacion)
                .WithMany(c => c.Mensajes)
                .HasForeignKey(m => m.ConversacionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Mensaje>()
                .HasOne(m => m.Remitente)
                .WithMany()
                .HasForeignKey(m => m.RemitenteId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Mensaje>()
                .HasIndex(m => new { m.ConversacionId, m.Fecha });

            // Bloqueo: una persona no puede bloquear dos veces a la misma
            modelBuilder.Entity<Bloqueo>()
                .HasOne(b => b.Bloqueador)
                .WithMany()
                .HasForeignKey(b => b.BloqueadorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Bloqueo>()
                .HasOne(b => b.Bloqueado)
                .WithMany()
                .HasForeignKey(b => b.BloqueadoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Bloqueo>()
                .HasIndex(b => new { b.BloqueadorId, b.BloqueadoId })
                .IsUnique();

            // Colaborador -> Solucion / Usuario (un usuario solo puede unirse una vez a cada solución)
            modelBuilder.Entity<Colaborador>()
                .HasOne(c => c.Solucion)
                .WithMany(s => s.Colaboradores)
                .HasForeignKey(c => c.SolucionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Colaborador>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Colaborador>()
                .HasIndex(c => new { c.SolucionId, c.UsuarioId })
                .IsUnique();

            // Avance -> Solucion / Usuario
            modelBuilder.Entity<Avance>()
                .HasOne(a => a.Solucion)
                .WithMany(s => s.Avances)
                .HasForeignKey(a => a.SolucionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Avance>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            // Comentario -> Publicacion
            // (NoAction: Usuario ya llega a Comentario en cascada por Solucion; al eliminar una
            //  publicación, PublicacionService borra sus comentarios manualmente)
            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Publicacion)
                .WithMany(p => p.Comentarios)
                .HasForeignKey(c => c.PublicacionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Comentario -> Comentario (respuestas). SQL Server no permite cascada en autorreferencias
            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.ComentarioPadre)
                .WithMany(c => c.Respuestas)
                .HasForeignKey(c => c.ComentarioPadreId)
                .OnDelete(DeleteBehavior.NoAction);

            // LikePublicacion -> Usuario / Publicacion
            modelBuilder.Entity<LikePublicacion>()
                .HasOne(l => l.Usuario)
                .WithMany()
                .HasForeignKey(l => l.Usuario_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LikePublicacion>()
                .HasOne(l => l.Publicacion)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.Publicacion_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // LikeComentario -> Usuario / Comentario
            modelBuilder.Entity<LikeComentario>()
                .HasOne(l => l.Usuario)
                .WithMany()
                .HasForeignKey(l => l.Usuario_Id)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LikeComentario>()
                .HasOne(l => l.Comentario)
                .WithMany(c => c.Likes)
                .HasForeignKey(l => l.Comentario_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Like -> Usuario
            modelBuilder.Entity<LikeSolucion>()
                .HasOne(l => l.Usuario)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.Usuario_Id)
                .OnDelete(DeleteBehavior.NoAction);

            // Like -> Solucion
            modelBuilder.Entity<LikeSolucion>()
                .HasOne(l => l.Solucion)
                .WithMany(s => s.Likes)
                .HasForeignKey(l => l.Solucion_Id)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }

    
}
