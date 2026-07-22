using Casor.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core + PostgreSQL (la caja usará el mismo DbContext con UseSqlite)
builder.Services.AddDbContext<CasorDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Central")));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/salud", () => Results.Ok(new { estado = "ok", fecha = DateTime.UtcNow }));

app.Run();
