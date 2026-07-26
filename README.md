# MiniMES

Plataforma simplificada de **MES / Shop Floor** para estudo de C#/.NET e arquitetura de
microsserviços industriais. Controla a execução de ordens de produção: produto → ordem →
operações → apontamento de produção/refugo → paradas → qualidade → conclusão.

> Especificação completa (visão, decisões e roadmap detalhado): [`MiniMES_Contexto_Tecnico_Completo.txt`](./MiniMES_Contexto_Tecnico_Completo.txt).
> Este README é o **checklist vivo**: o que já existe vs. o que falta.

## Stack

.NET 10 · C# 14 · ASP.NET Core Web API (controllers) · EF Core 10 (Npgsql) · PostgreSQL 18 ·
xUnit · Docker Compose. Futuro: NATS/JetStream, Keycloak, SignalR, OpenTelemetry, Nomad.

## Arquitetura — microsserviços por contexto

Cada domínio é um serviço com **projeto e banco próprios** (sem tabelas compartilhadas);
referências entre serviços são `Guid` solto, validadas por API/evento. Construídos **um a um**.

| Serviço | Projeto | Entidades | Banco | Status |
|---|---|---|---|---|
| **Production** | `src/MiniMes.Production` | Product, ProductionOrder, ProductionOperation | `production_db` | 🟡 em construção |
| Execution | `src/MiniMes.Execution` | OperationExecution, ProductionReport | `execution_db` | ⬜ |
| Equipment | `src/MiniMes.Equipment` | Equipment, DowntimeEvent | `equipment_db` | ⬜ |
| Quality | `src/MiniMes.Quality` | QualityInspection, QualityMeasurement | `quality_db` | ⬜ |
| Realtime | `src/MiniMes.Realtime` | SignalR alimentado por NATS | — | ⬜ |
| OutboxPublisher / MachineSimulator | `workers/` | workers | — | ⬜ |

Código compartilhado (`shared/MiniMes.BuildingBlocks`) só é extraído quando o **2º serviço**
nascer — hoje o handler de exceção/health check vivem dentro de `MiniMes.Production`.

Camadas dentro de cada serviço: `Controller → Service → Repository → DbContext → PostgreSQL`,
com **domínio rico** (regras nas entidades, sem setter público de estado) e **um `SaveChanges`**
por caso de uso (transação automática do EF).

## Como rodar

API e banco sobem juntos pelo Compose, com **hot reload** (`dotnet watch` dentro do container,
código bind-montado do host). As migrations são aplicadas no startup em `Development`.

```bash
docker compose up                  # API em :5033 (hot reload) + Postgres em :5434
curl http://localhost:5033/health  # → Healthy
```

Cada serviço tem **seu próprio par api + banco** no `compose.yaml`; para o próximo, copie o bloco e
troque nome, porta da API, porta do banco e `Database=`.

Para rodar a API fora do container (ou usar `dotnet ef` do host), a connection string vem de
user-secrets — o Postgres do Compose continua exposto em `localhost:5434`:

```bash
dotnet user-secrets set "ConnectionStrings:Postgres" \
  "Host=localhost;Port=5434;Database=production_db;Username=minimes;Password=<senha>" \
  --project src/MiniMes.Production/MiniMes.Production.csproj

dotnet tool restore
dotnet run --project src/MiniMes.Production   # http://localhost:5033
dotnet test                                    # testes unitários
```

Endpoints de exemplo em [`src/MiniMes.Production/MiniMes.Production.http`](./src/MiniMes.Production/MiniMes.Production.http).

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
| `POST /api/production-orders/{id}/operations` | ⬜ (falta `ProductionOperation`) |
| `POST /api/operations/{id}/start` | ⬜ |
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
- [ ] `ProductionOperation` (sequência, operações da ordem)
- [ ] Regra: liberar só com ≥1 operação; concluir só com todas as operações concluídas

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
