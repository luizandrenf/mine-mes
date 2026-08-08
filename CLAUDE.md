# CLAUDE.md

Guidance for AI agents working in this repo.

## Overview

MiniMES — a simplified MES / Shop Floor study project (C#/.NET) built as **microservices, one at a
time**. Full spec: [`MiniMES_Contexto_Tecnico_Completo.txt`](./MiniMES_Contexto_Tecnico_Completo.txt).
Living progress checklist: [`README.md`](./README.md).

Monorepo: **`backend/`** holds the .NET solution (services + tests), **`frontend/`** the Next.js web
app. One `compose.yaml` at the root brings up everything.

## Architecture

One service per bounded context, **own project and own database** (no shared tables); cross-service
references are loose `Guid`s validated via API/event, never a FK across databases.

- Current service: `backend/src/MiniMes.Production` (owns Product, ProductionOrder, ProductionOperation).
- Future: Execution, Equipment, Quality, Realtime services + Outbox/Simulator workers.
- Shared code (`backend/shared/MiniMes.BuildingBlocks`) is extracted only when the **2nd** service exists.

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

## Frontend (`frontend/`)

Next.js 16 (App Router) · React 19 · MUI v9 · TypeScript · Vitest. Read
[`frontend/AGENTS.md`](./frontend/AGENTS.md) before writing Next code — this version differs from
what training data assumes.

- **MUI v9, not v5/v6.** No shorthand system props on components (`display=`, `gap=`, `fontWeight=`)
  — everything goes through `sx`. `TextField` takes `slotProps`, not `InputProps`. `Button` has its
  own `loading` prop. There is no Tailwind and no theme file: the default theme plus `CssBaseline`
  is the whole styling setup.
- MUI components ship `"use client"`, so a Server Component can render them. What it **cannot** do
  is pass `component={NextLink}` across the boundary — `components/nav.tsx` holds the two client
  wrappers (`AppLink`, `NavButton`) that keep `next/link` routing.

- **No client-side data fetching.** Reads are `await service.getX()` inside a Server Component;
  writes are Server Actions plus `revalidatePath`. Only the Node process talks to the API, which is
  why the API needs no CORS policy and `API_BASE_URL` is never a `NEXT_PUBLIC_` variable.
- **Layers mirror the backend**: `page (RSC) → Server Action → Service → HttpClient → API`.
  `HttpClient` is an interface so a test swaps in `FakeHttpClient` — same rule as the backend, fakes
  by hand, no mock library.
- `lib/domain/transitions.ts` mirrors the entity invariants **only to disable buttons**. The domain
  stays the authority: whatever slips through is refused by the API and the ProblemDetails `detail`
  is rendered as-is, next to the button that caused it.
- Errors: `runAction` turns any `ApiError` below 500 into form state; everything else is rethrown to
  `app/error.tsx`.
- Types in `lib/api/types.ts` are hand-mirrored from the DTOs (no codegen) — change a DTO, change
  them too.
- English everywhere, comments only where indispensable (same rule as C#). Run
  `npm run lint && npm test && npm run build` before finishing.

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
docker compose up -d          # Postgres :5434 + API :5033 + web :3000 (see .env)

# backend/ — dotnet commands run from there
dotnet tool restore                                    # dotnet-ef, csharpier (local tools)
dotnet build MiniMes.sln
dotnet test                                            # xUnit
dotnet run --project src/MiniMes.Production            # http://localhost:5033, /health
dotnet ef migrations add <Name> --project src/MiniMes.Production
dotnet ef database update --project src/MiniMes.Production

# frontend/
npm install
npm run dev                   # http://localhost:3000, reads API_BASE_URL (.env.local.example)
npm run lint && npm test && npm run build
```

## Notes

- **EF Core pinned to 10.0.10** (`Relational` + `Design`) because the Npgsql provider (10.0.3) pulls
  10.0.4 and the test project otherwise ends up with conflicting versions.
- **Tests**: xUnit with hand-written fakes (no mock library). Entity tests + service tests. The
  frontend follows the same shape under Vitest — `lib/test/fake-http-client.ts` is the counterpart
  of the fake repositories.
