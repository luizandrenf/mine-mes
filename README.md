# MiniMES

Plataforma simplificada de **MES / Shop Floor** para estudo de C#/.NET e arquitetura de
microsserviços industriais. Controla a execução de ordens de produção: produto → ordem →
operações → apontamento de produção/refugo → paradas → qualidade → conclusão.

> Especificação completa (visão, decisões e roadmap detalhado): [`MiniMES_Contexto_Tecnico_Completo.txt`](./MiniMES_Contexto_Tecnico_Completo.txt).
> Este README é o **checklist vivo**: o que já existe vs. o que falta.

## Stack

**Backend** — .NET 10 · C# 14 · ASP.NET Core Web API (controllers) · EF Core 10 (Npgsql) ·
PostgreSQL 18 · xUnit.
**Frontend** — Next.js 16 (App Router) · React 19 · MUI v9 · TypeScript · Vitest.
Tudo sobe junto por Docker Compose. Futuro: NATS/JetStream, Keycloak, SignalR, OpenTelemetry, Nomad.

## Estrutura — monorepo

```
backend/    solução .NET: src/<serviço> + tests/ + MiniMes.sln
frontend/   web app Next.js (fala só com a API do Production)
compose.yaml  banco + API + web
```

## Arquitetura — microsserviços por contexto

Cada domínio é um serviço com **projeto e banco próprios** (sem tabelas compartilhadas);
referências entre serviços são `Guid` solto, validadas por API/evento. Construídos **um a um**.

| Serviço | Projeto | Entidades | Banco | Status |
|---|---|---|---|---|
| **Production** | `backend/src/MiniMes.Production` | Product, ProductionOrder, ProductionOperation | `production_db` | 🟡 em construção |
| Execution | `backend/src/MiniMes.Execution` | OperationExecution, ProductionReport | `execution_db` | ⬜ |
| Equipment | `backend/src/MiniMes.Equipment` | Equipment, DowntimeEvent | `equipment_db` | ⬜ |
| Quality | `backend/src/MiniMes.Quality` | QualityInspection, QualityMeasurement | `quality_db` | ⬜ |
| Realtime | `backend/src/MiniMes.Realtime` | SignalR alimentado por NATS | — | ⬜ |
| OutboxPublisher / MachineSimulator | `backend/workers/` | workers | — | ⬜ |

Código compartilhado (`backend/shared/MiniMes.BuildingBlocks`) só é extraído quando o **2º serviço**
nascer — hoje o handler de exceção/health check vivem dentro de `MiniMes.Production`.

Camadas dentro de cada serviço: `Controller → Service → Repository → DbContext → PostgreSQL`,
com **domínio rico** (regras nas entidades, sem setter público de estado) e **um `SaveChanges`**
por caso de uso (transação automática do EF).

## Como rodar

API e banco sobem juntos pelo Compose, com **hot reload** (`dotnet watch` dentro do container,
código bind-montado do host). As migrations são aplicadas no startup em `Development`.

```bash
docker compose up                  # web :3000 + API :5033 (hot reload) + Postgres :5434
curl http://localhost:5033/health  # → Healthy
```

Cada serviço tem **seu próprio par api + banco** no `compose.yaml`; para o próximo, copie o bloco e
troque nome, porta da API, porta do banco e `Database=`.

Para rodar a API fora do container (ou usar `dotnet ef` do host), a connection string vem de
user-secrets — o Postgres do Compose continua exposto em `localhost:5434`:

```bash
cd backend

dotnet user-secrets set "ConnectionStrings:Postgres" \
  "Host=localhost;Port=5434;Database=production_db;Username=minimes;Password=<senha>" \
  --project src/MiniMes.Production/MiniMes.Production.csproj

dotnet tool restore
dotnet run --project src/MiniMes.Production   # http://localhost:5033
dotnet test                                    # testes unitários
```

Endpoints de exemplo em [`backend/src/MiniMes.Production/MiniMes.Production.http`](./backend/src/MiniMes.Production/MiniMes.Production.http).

## Frontend

Interface web do Production em [`frontend/`](./frontend): produtos, ordens e operações, com as
transições de estado expostas como botões. Todo dado é lido no servidor (React Server Components) e
toda escrita é Server Action — **o browser nunca chama a API**, por isso ela não precisa de CORS.

O `docker compose up` já sobe em `http://localhost:3000`. Fora do container:

```bash
cd frontend
cp .env.local.example .env.local   # API_BASE_URL=http://localhost:5033
npm install
npm run dev                        # http://localhost:3000
npm test                           # Vitest
```

### Debug com breakpoints no container

O VS Code anexa no processo dentro do container (`.vscode/launch.json`). Passo único de setup —
instala o debugger num volume, então sobrevive a `docker compose up --force-recreate`:

```bash
docker compose exec production-api \
  bash -c 'curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg'
```

Depois: **F5 → "Attach: production-api (docker)"** e escolher o processo
`/app/bin/Debug/net10.0/MiniMes.Production`.

Há dois modos de rodar, e a escolha é na subida:

| Modo | Comando | Ao salvar um arquivo |
|---|---|---|
| Hot reload (padrão) | `docker compose up -d` | aplica a mudança na hora, sem restart |
| Debug | `docker compose -f compose.yaml -f compose.debug.yaml up -d` | **nada** — o processo fica parado |

O modo debug troca o `dotnet watch` por `dotnet run` puro: sem watch o processo não reinicia, e
restart derruba o debugger no meio da sessão. Em compensação, mudou o código, tem que recriar:

```bash
docker compose -f compose.yaml -f compose.debug.yaml up -d --force-recreate production-api
```

O `compose.yaml` concede `SYS_PTRACE` e `seccomp:unconfined` ao serviço — o vsdbg não anexa sem
isso. É afrouxamento de isolamento aceitável em dev, que não deve viajar para produção.

## Endpoints do MVP

✅ pronto · 🟡 domínio pronto, HTTP ausente · ⬜ pendente

| Endpoint | Status |
|---|---|
| `POST /api/products` | ✅ |
| `GET /api/products` · `GET /api/products/{id}` | ✅ |
| `PATCH /api/products/{id}/activate` · `/deactivate` | ✅ |
| `POST /api/production-orders` | ✅ (valida produto existente + ativo e número duplicado) |
| `GET /api/production-orders` · `GET /api/production-orders/{id}` | ✅ |
| `POST /api/production-orders/{id}/release` | ✅ |
| `POST /api/production-orders/{id}/start` · `/complete` | ✅ (provisório: virá do Execution) |
| `POST /api/production-orders/{id}/cancel` | ✅ |
| `POST /api/production-orders/{id}/operations` | ✅ (sequência única, só em `Draft`) |
| `POST /api/production-orders/{id}/operations/{operationId}/start` · `/complete` · `/cancel` | ✅ (provisório: virá do Execution) |
| `POST /api/operations/{id}/start` | ⬜ (rota do serviço Execution) |
| `POST /api/executions/{id}/reports` | ⬜ |
| `POST /api/executions/{id}/complete` | ⬜ |
| `GET /api/executions/{id}` | ⬜ |

## Checklist por fase

### Fase 1 — Fundação
- [x] PostgreSQL via Docker Compose
- [x] EF Core + migration inicial + configuração por entidade
- [x] DbContext pequeno (`ApplyConfigurationsFromAssembly`)
- [x] UnitOfWork
- [x] Handler global de exceção → ProblemDetails (422 domínio · 404 not found · 409 concorrência · 500)
- [x] Health check (`/health`, checa o DbContext)
- [ ] Teste de integração inicial (WebApplicationFactory + Testcontainers)

### Fase 2 — Product
- [x] Entidade `Product` (regras: code/name obrigatórios; `Activate`/`Deactivate`)
- [x] Configuration + migration (`products`, `code` único)
- [x] Repository + Service + Controller
- [x] FK `ProductionOrder.ProductId → products`; ordem valida produto ativo
- [x] Testes unitários (entidade + service)

### Fase 3 — ProductionOrder
- [x] Entidade rica com status e transições (Draft→Released→InProgress→Completed / Cancelled)
- [x] Concorrência otimista (`Version`)
- [x] Criar / listar / obter por id
- [x] Expor `release` / `start` / `complete` / `cancel` via HTTP (controller + service)
- [x] `ProductionOperation` (sequência, operações da ordem)
- [x] Regra: liberar só com ≥1 operação; concluir só com todas as operações concluídas

### Fase 3.5 — Frontend do Production
- [x] Monorepo (`backend/` + `frontend/`), serviço `web` no Compose
- [x] Camadas espelhando o backend: `page (RSC) → Server Action → Service → HttpClient`
- [x] Produtos: listar, criar, ativar/desativar
- [x] Ordens: listar, criar, detalhe · release / start / complete / cancel
- [x] Operações: adicionar (só em `Draft`) · start / complete / cancel, respeitando a sequência
- [x] Botão desabilitado onde a invariante do domínio já refuta; `detail` do ProblemDetails na tela
- [x] Testes (Vitest + Testing Library, fakes à mão) · boundaries `error` / `not-found`
- [ ] Editar/excluir, filtro, paginação e busca (a API também não tem)
- [ ] Testes de página / E2E · imagem Docker de produção (hoje o container é só dev)
- [ ] Autenticação — depende da Fase 7

### Fase 4 — Execution (serviço próprio)
- [ ] `OperationExecution`, `ProductionReport` · start/pause/resume/report/complete · idempotência (`ClientEventId`)

### Fase 5 — Equipment (serviço próprio)
- [ ] `Equipment`, `DowntimeEvent` · estado, paradas

### Fase 6 — Quality (serviço próprio)
- [ ] `QualityInspection`, `QualityMeasurement` · inspeção, medição, aprovação/reprovação

### Fase 7 — Segurança
- [ ] Keycloak (OIDC/JWT), autorização por roles, auditoria

### Fase 8 — Mensageria
- [ ] Transactional Outbox · NATS/JetStream · `ProcessedMessage` (idempotência)

### Fase 9 — Tempo real e observabilidade
- [ ] SignalR · OpenTelemetry · métricas · dashboards

### Fase 10 — Microsserviços e Nomad
- [ ] Extrair `BuildingBlocks` · deploy on-premises · jobs Nomad

## Convenções

- **Controller fino**: HTTP, rotas, contratos e status — nada de EF Core, regra ou `SaveChanges`.
- **Regra no domínio**: estado sem setter público; transição por método (`order.Release()`),
  que valida a invariante e lança `DomainException`.
- **Request → Command → Entidade → DTO**: o request HTTP não entra no service; o DTO não expõe
  entidade nem o `Version`.
- **Um `SaveChanges`** por caso de uso (via `IUnitOfWork`) para uma transação só.
- **Tracking consciente**: leitura com `AsNoTracking`; alteração carrega rastreada (`...ForUpdateAsync`).
- **Concorrência otimista**: `Version` como token; `DbUpdateConcurrencyException` → HTTP 409.
