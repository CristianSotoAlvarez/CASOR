using System.Text;
using Casor.Api.Endpoints;
using Casor.Application.Abastecimiento;
using Casor.Application.Auth;
using Casor.Application.Catalogo;
using Casor.Application.Comun;
using Casor.Application.Stock;
using Casor.Domain;
using Casor.Domain.Entidades;
using Casor.Infrastructure;
using Casor.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios (inyección de dependencias) ----------
builder.Services.AddDbContext<CasorDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Central")));

builder.Services.AddScoped<ICasorDb>(sp => sp.GetRequiredService<CasorDbContext>());
builder.Services.AddScoped<CrearProducto>();
builder.Services.AddScoped<CambiarPrecio>();
builder.Services.AddScoped<BuscarProductos>();
builder.Services.AddScoped<AlternativasBioequivalentes>();
builder.Services.AddScoped<RegistrarMovimientoStock>();
builder.Services.AddScoped<ConsultarAlertas>();
builder.Services.AddScoped<RegistrarRecepcion>();
builder.Services.AddScoped<RegistrarAjuste>();
builder.Services.AddScoped<IServicioPassword, ServicioPassword>();
builder.Services.AddScoped<IServicioToken, ServicioTokenJwt>();

// ---------- Autenticación: cómo se valida cada carnet entrante ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,   // la firma debe calzar con nuestra clave
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,           // token vencido = rechazado
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------- Pipeline: el orden IMPORTA ----------
app.UseAuthentication();   // 1º identifica quién viene (lee el carnet)
app.UseAuthorization();    // 2º decide si puede pasar (revisa el rol)

// ---------- Endpoints ----------
app.MapGet("/salud", () => Results.Ok(new { estado = "ok", fecha = DateTime.UtcNow }));

// LOGIN: público por definición (es la puerta de entrada)
app.MapPost("/auth/login", async (LoginRequest req, CasorDbContext db,
    IServicioPassword pwd, IServicioToken tokens) =>
{
    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Username == req.Username && u.Activo);

    // Mismo mensaje si el usuario no existe o la clave falla:
    // no le regalamos pistas a un atacante sobre qué usuarios existen.
    if (usuario is null || !pwd.Verificar(req.Password, usuario.PasswordHash))
        return Results.Unauthorized();

    var (token, expira) = tokens.CrearToken(usuario);
    return Results.Ok(new LoginResponse(token, usuario.Username, usuario.Rol.ToString(), expira));
});

// Ejemplos de la segregación RNF-03 (los módulos reales los reemplazarán):
app.MapGet("/admin/ping", () => Results.Ok(new { zona = "admin" }))
   .RequireAuthorization(p => p.RequireRole(nameof(RolUsuario.Admin)));

app.MapGet("/pos/ping", () => Results.Ok(new { zona = "pos" }))
   .RequireAuthorization(p => p.RequireRole(nameof(RolUsuario.Admin), nameof(RolUsuario.Pos)));

app.MapInventario();
app.MapAbastecimiento();

// ---------- Semilla: primer admin si la base está vacía ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CasorDbContext>();
    var pwd = scope.ServiceProvider.GetRequiredService<IServicioPassword>();
    if (db.Database.CanConnect() && !db.Usuarios.Any())
    {
        db.Usuarios.Add(new Usuario
        {
            Nombre = "Administrador inicial",
            Username = "admin",
            PasswordHash = pwd.Hashear("admin123"),
            Rol = RolUsuario.Admin
        });
        db.SaveChanges();
        app.Logger.LogWarning("Usuario semilla creado: admin/admin123 — CAMBIAR antes de producción");
    }
}

app.Run();
