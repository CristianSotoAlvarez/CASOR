namespace Casor.Application.Auth;

/// <summary>Lo que envía el POS/Admin al hacer login.</summary>
public record LoginRequest(string Username, string Password);

/// <summary>Lo que devuelve la API: el "carnet" (JWT) y sus datos visibles.</summary>
public record LoginResponse(string Token, string Username, string Rol, DateTimeOffset ExpiraEn);
