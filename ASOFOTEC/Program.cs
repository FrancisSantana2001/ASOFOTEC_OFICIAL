using ASOFOTEC.Data;
using ASOFOTEC.Services.Implementations;
using ASOFOTEC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using ASOFOTEC.Hubs;

var builder = WebApplication.CreateBuilder(args);

//Configurar la cadena de conexion a la base de datos
var ConnetionStrings = builder.Configuration.GetConnectionString("ASOFOTECdbConnetion");
builder.Services.AddDbContext<AppDbContext>(Op => Op.UseSqlServer(ConnetionStrings));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Chat en tiempo real
builder.Services.AddSignalR();
builder.Services.AddScoped<IProblemaService, ProblemaService>();
builder.Services.AddScoped<IsolucionService, SolucionService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IComentarioService, ComentarioService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IPublicacionService, PublicacionService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IRutaProblemaService, RutaProblemaService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<ISeguimientoService, SeguimientoService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IBloqueoService, BloqueoService>();
builder.Services.AddScoped<IFeedService, FeedService>();

//Agregar servicios de autenticacion JWT
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

//Configuracion de autenticacion JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key  = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Leer JWT desde la cookie "jwt"
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Archivos estáticos (css, js, imágenes, fotos subidas): no hace falta validar el token
                if (Path.HasExtension(context.Request.Path))
                {
                    return Task.CompletedTask;
                }

                context.Token = context.Request.Cookies["Jwt"];
                return Task.CompletedTask;
            },

            // Un token firmado no basta: la cuenta debe seguir existiendo y estar activa.
            // Así, desactivar a un usuario corta su acceso de inmediato.
            OnTokenValidated = async context =>
            {
                var valor = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(valor, out var usuarioId))
                {
                    context.Fail("Token sin usuario.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var activo = await db.usuarios
                    .AsNoTracking()
                    .Where(u => u.Id == usuarioId)
                    .Select(u => (bool?)u.Activo)
                    .FirstOrDefaultAsync();

                if (activo != true)
                {
                    context.Fail("Usuario inexistente o desactivado.");
                }
            },

            // Usuario sin sesión intentando usar una acción protegida:
            // - peticiones AJAX (fetch) -> 401 para que el JS redirija al login
            // - navegación normal       -> redirige a la pantalla de login
            OnChallenge = context =>
            {
                context.HandleResponse();

                if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    // Se recuerda la página a la que quería entrar para volver ahí tras iniciar sesión.
                    // Si era un POST (no se puede repetir con un GET) se usa la página de origen.
                    var request = context.Request;
                    var destino = "/";

                    if (HttpMethods.IsGet(request.Method))
                    {
                        destino = request.Path + request.QueryString;
                    }
                    else if (Uri.TryCreate(request.Headers.Referer.ToString(), UriKind.Absolute, out var origen)
                             && string.Equals(origen.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase))
                    {
                        destino = origen.PathAndQuery;
                    }

                    context.Response.Redirect("/Auth/Login?returnUrl=" + Uri.EscapeDataString(destino));
                }
                return Task.CompletedTask;
            },

            // Autenticado pero sin permiso
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });
           



//Configurar y aplicar sesion de usuario
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Garantiza los roles y el administrador DBA (si está configurado)
await ASOFOTEC.Data.DbInicializador.AsegurarRolesYDbaAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Sirve los archivos que se suben mientras la aplicación está en ejecución (fotos y videos)
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();

// Punto de conexión del chat (requiere sesión iniciada)
app.MapHub<ChatHub>("/hubs/chat");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
