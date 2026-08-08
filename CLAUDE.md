# CLAUDE.md

Guidance for AI agents working in this repo.

## Overview

MiniMES — a simplified MES / Shop Floor study project (C#/.NET) built as **microservices, one at a
time**. Full spec: [`MiniMES_Contexto_Tecnico_Completo.txt`](./MiniMES_Contexto_Tecnico_Completo.txt).
Living progress checklist: [`README.md`](./README.md).

## Architecture

One service per bounded context, **own project and own database** (no shared tables); cross-service
references are loose `Guid`s validated via API/event, never a FK across databases.

- Current service: `src/MiniMes.Production` (owns Product, ProductionOrder, ProductionOperation).
- Future: Execution, Equipment, Quality, Realtime services + Outbox/Simulator workers.
- Shared code (`shared/MiniMes.BuildingBlocks`) is extracted only when the **2nd** service exists.

Layers inside a service: `Controller → Service → Repository → DbContext → PostgreSQL`.

## Conventions

- **Thin controller**: HTTP, routing, contracts, status codes only. No EF Core, no domain rules, no `SaveChanges`.
- **Rich domain**: no public state setters; transitions go through methods (`order.Release()`) that
  validate invariants and throw `DomainException`.
- **One `SaveChanges` per use case** via `IUnitOfWork` (single transaction).
- **Request → Command → Entity → DTO**: the HTTP request never reaches the service; the DTO never
  exposes the entity or the `Version` token.
- **Tracking**: reads use `AsNoTracking`; writes load tracked (`...ForUpdateAsync`).
- **Optimistic concurrency**: `Version` token; `DbUpdateConcurrencyException` → HTTP 409.
- **Errors**: `DomainExceptionHandler` maps to ProblemDetails — 422 domain, 404 not found, 409
  concurrency, 500 fallback.

## Code style (required)

- **Language**: all code — comments **and** exception/user-facing messages — in **English**.
  The `README.md` stays in **Portuguese**.
- **Comments**: only where indispensable (a non-obvious decision). No learner notes; the conventions
  above live here, not inline.
- **Modern C#**: use **primary constructors** for DI/infra/service/handler/exception classes (use the
  parameter directly, no redundant `_field`). Keep a hand-written constructor only when it validates
  invariants (entities) or the EF Core parameterless private ctor is required.
- **XML doc comments** (`///`) are the exception to the "comments only where indispensable" rule:
  every controller action and request contract property carries them, because they feed the OpenAPI
  document (`GenerateDocumentationFile`) and show up in Swagger UI. Nowhere else.
- Records for DTOs/Commands. Run `dotnet csharpier format .` before finishing.

## Commands

```bash
docker compose up -d                                   # PostgreSQL (see .env, port 5434)
dotnet tool restore                                    # dotnet-ef, csharpier (local tools)
dotnet build MiniMes.sln
dotnet test                                            # xUnit
dotnet run --project src/MiniMes.Production            # http://localhost:5033, /health

# EF Core migrations (run from repo root)
dotnet ef migrations add <Name> --project src/MiniMes.Production
dotnet ef database update --project src/MiniMes.Production
```

## Notes

- **EF Core pinned to 10.0.10** (`Relational` + `Design`) because the Npgsql provider (10.0.3) pulls
  10.0.4 and the test project otherwise ends up with conflicting versions.
- **Tests**: xUnit with hand-written fakes (no mock library). Entity tests + service tests.
