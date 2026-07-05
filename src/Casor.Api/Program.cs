var builder = WebApplication.CreateBuilder(args);

// TODO Fase 1: EF Core + PostgreSQL, JWT, roles Admin/POS (RNF-03)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/salud", () => Results.Ok(new { estado = "ok", fecha = DateTime.UtcNow }));

app.Run();
