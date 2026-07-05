# CASOR — Sistema de Gestión para Farmacias

POS + administración para farmacias: inventario por lote con FEFO, motor de
bioequivalentes, cumplimiento ISP, caja X/Z y reportería. Escritorio offline-first
(Avalonia), servidor central ASP.NET Core + PostgreSQL.

> Plan de trabajo y arquitectura completa: folder **Proyecto CASOR** en ClickUp
> (Doc "CASOR · Arquitectura y Pautas Técnicas").

## Estructura

```
Casor.sln
├── src/
│   ├── Casor.Domain/          ← reglas de negocio puras (FEFO, IVA, promociones)
│   ├── Casor.Application/     ← casos de uso (RegistrarVenta, CerrarCajaZ...)
│   ├── Casor.Infrastructure/  ← EF Core (PostgreSQL/SQLite), sync, impresión, SII
│   ├── Casor.Api/             ← servidor central (ASP.NET Core, JWT)
│   ├── Casor.Pos/             ← cliente cajero (Avalonia, MVVM)
│   └── Casor.Admin/           ← cliente gerente (Avalonia, MVVM)
└── tests/
    └── Casor.Tests/           ← xUnit; aquí vive la garantía del inventario
```

**Regla de dependencias:** Domain no depende de nada. Application solo de Domain.
La UI y la Api nunca saltan capas.

## Puesta en marcha (cada integrante, Fase 0)

```bash
# 1. Requisitos: .NET 10 SDK, Docker Desktop, IDE (Rider / VS / VS Code)
git clone <url-del-repo> && cd casor

# 2. Base de datos local
docker compose up -d

# 3. Compilar y correr tests
dotnet build
dotnet test

# 4. Levantar la API
dotnet run --project src/Casor.Api    # GET http://localhost:5000/salud

# 5. Generar las apps Avalonia reales (una sola vez, reemplaza los Program.cs placeholder)
dotnet new install Avalonia.Templates
dotnet new avalonia.mvvm -o src/Casor.Pos --force
dotnet new avalonia.mvvm -o src/Casor.Admin --force
```

## Cómo contribuir

Leer **CONTRIBUTING.md** (ramas, commits, PRs, reglas de oro). Resumen: nada
entra a `main` sin PR aprobado y tests en verde.

## Hitos

V1.0 → 23-oct-2026, farmacia piloto. Detalle en ClickUp, página
"7. Cronograma e Hitos".
