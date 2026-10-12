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
