# PLAN — FinTrack API (.NET + Clean Architecture + PostgreSQL)

> Plano de execução baseado no roteiro `roteiro .net.txt`, com as seguintes adaptações:
>
> | Roteiro original | Neste projeto |
> |---|---|
> | Docker Desktop | **Rancher Desktop** (engine `dockerd (moby)`, então os comandos `docker` / `docker compose` funcionam iguais) |
> | SQLite (`Microsoft.EntityFrameworkCore.Sqlite`) | **PostgreSQL** (`Npgsql.EntityFrameworkCore.PostgreSQL`) rodando em container |
> | SQLite Viewer (extensão) | Extensão **PostgreSQL** (Microsoft) ou DBeaver/pgAdmin |
> | Clean Arch "simplificada" (Core / Infrastructure / Api) | **Clean Architecture** com 4 camadas (Domain / Application / Infrastructure / Api) |
>
> Repositório: https://github.com/DaviLGS/FinTrackApi-.net.git
> Editor: VS Code

---

## 1. Objetivo

Construir uma API REST de controle financeiro pessoal (**FinTrack**) que permita cadastrar e consultar **transações** financeiras, vinculadas a **contas** e **categorias**, com:

- Persistência em PostgreSQL via Entity Framework Core (migrations + seed data);
- Validação de entrada com FluentValidation;
- Documentação interativa com Swagger;
- Testes unitários com xUnit;
- Execução containerizada (API + banco) com `docker compose` no Rancher Desktop.

---

## 2. Stack e decisões técnicas

| Item | Escolha | Observação |
|---|---|---|
| Runtime | **.NET 10 (LTS)** — mínimo .NET 8 | O roteiro pede 8.0+; usar a LTS mais recente instalada. Todos os projetos devem ter o mesmo `TargetFramework`. |
| Web | ASP.NET Core Web API com **Controllers** | Template `dotnet new webapi --use-controllers`. |
| Documentação | **Swashbuckle.AspNetCore** (Swagger UI) | A partir do .NET 9 o template não traz Swagger UI; adicionar o pacote. |
| ORM | EF Core + **Npgsql.EntityFrameworkCore.PostgreSQL** | `Microsoft.EntityFrameworkCore.Design` na Api para o `dotnet ef`. |
| Validação | **FluentValidation** + `FluentValidation.DependencyInjectionExtensions` | `FluentValidation.AspNetCore` (auto-validation) está descontinuado; validar explicitamente no service/endpoint. |
| Testes | xUnit + **NSubstitute** (mocks) | Assertions nativas do xUnit (ou Shouldly). |
| Banco | PostgreSQL 16 (imagem `postgres:16-alpine`) | Container local via compose. |
| Containers | Rancher Desktop (moby) | `docker` CLI + `docker compose`. |
| Ferramenta EF | `dotnet-ef` (tool global ou local) | `dotnet tool install --global dotnet-ef`. |

---

## 3. Arquitetura (Clean Architecture)

Regra de dependência: as setas apontam **para dentro**. O Domain não conhece ninguém.

```
           ┌──────────────────────────┐
           │       FinTrack.Api       │  Controllers, Middlewares, Swagger, DI (composition root)
           └──────┬────────────┬──────┘
                  │            │
                  ▼            ▼
┌───────────────────────┐   ┌──────────────────────────────┐
│ FinTrack.Application  │◄──│   FinTrack.Infrastructure    │  EF Core, DbContext, Repositórios, Migrations
│ DTOs, Services,       │   └──────────────┬───────────────┘
│ Validators, Interfaces│                  │
└──────────┬────────────┘                  │
           ▼                               ▼
        ┌──────────────────────────────────────┐
        │           FinTrack.Domain            │  Entidades, Enums, regras de domínio
        └──────────────────────────────────────┘
```

### Referências entre projetos

| Projeto | Referencia |
|---|---|
| `FinTrack.Domain` | — |
| `FinTrack.Application` | Domain |
| `FinTrack.Infrastructure` | Application, Domain |
| `FinTrack.Api` | Application, Infrastructure |
| `FinTrack.Tests` | Domain, Application (e Api/Infrastructure se necessário) |

### Estrutura de pastas alvo

```
FinTrackApi-.net/
├── FinTrack.sln
├── docker-compose.yml
├── .env.example
├── .gitignore
├── .editorconfig
├── README.md
├── PLAN.md
├── tasks.md
├── src/
│   ├── FinTrack.Domain/
│   │   ├── Entities/        Categoria.cs, Conta.cs, Transacao.cs, EntityBase.cs
│   │   └── Enums/           TipoTransacao.cs
│   ├── FinTrack.Application/
│   │   ├── DTOs/            TransacaoCreateDto.cs, TransacaoResponseDto.cs, ...
│   │   ├── Interfaces/      ITransacaoRepository.cs, ICategoriaRepository.cs, IContaRepository.cs, ITransacaoService.cs
│   │   ├── Services/        TransacaoService.cs
│   │   ├── Validators/      TransacaoCreateValidator.cs
│   │   ├── Common/          Result / exceções de aplicação, PagedResult
│   │   └── DependencyInjection.cs   (AddApplication)
│   ├── FinTrack.Infrastructure/
│   │   ├── Context/         AppDbContext.cs
│   │   ├── Configurations/  CategoriaConfiguration.cs, ContaConfiguration.cs, TransacaoConfiguration.cs
│   │   ├── Repositories/    TransacaoRepository.cs, CategoriaRepository.cs, ContaRepository.cs
│   │   ├── Migrations/
│   │   └── DependencyInjection.cs   (AddInfrastructure)
│   └── FinTrack.Api/
│       ├── Controllers/     TransacoesController.cs, CategoriasController.cs, ContasController.cs
│       ├── Middlewares/     ExceptionHandlingMiddleware.cs
│       ├── Program.cs
│       ├── appsettings.json / appsettings.Development.json
│       └── Dockerfile
└── tests/
    └── FinTrack.Tests/
        ├── Validators/      TransacaoCreateValidatorTests.cs
        └── Services/        TransacaoServiceTests.cs
```

---

## 4. Modelo de domínio

### Entidades

**Categoria**
| Campo | Tipo | Regras |
|---|---|---|
| Id | int | PK |
| Nome | string | obrigatório, máx. 100, único |
| Tipo | TipoTransacao | `Receita` ou `Despesa` |

**Conta**
| Campo | Tipo | Regras |
|---|---|---|
| Id | int | PK |
| Nome | string | obrigatório, máx. 100 |
| SaldoInicial | decimal(18,2) | ≥ 0 |

**Transacao**
| Campo | Tipo | Regras |
|---|---|---|
| Id | int | PK |
| Descricao | string | obrigatório, máx. 200 |
| Valor | decimal(18,2) | > 0 |
| Data | DateTime (UTC) | obrigatória, não pode ser futura |
| Tipo | TipoTransacao | `Receita` ou `Despesa` |
| CategoriaId | int | FK → Categoria (obrigatória, deve existir) |
| ContaId | int | FK → Conta (obrigatória, deve existir) |
| CriadoEm | DateTime (UTC) | preenchido automaticamente |

**Enum `TipoTransacao`**: `Receita = 1`, `Despesa = 2`.

### Relacionamentos
- Categoria 1 — N Transacao (`OnDelete: Restrict`)
- Conta 1 — N Transacao (`OnDelete: Restrict`)

### Seed data
- Categorias: Salário (Receita), Freelance (Receita), Alimentação (Despesa), Transporte (Despesa), Moradia (Despesa), Lazer (Despesa).
- Contas: Carteira, Conta Corrente.

### Regras de negócio
1. `Valor` sempre positivo; o sinal é dado pelo `Tipo`.
2. O `Tipo` da transação deve ser compatível com o `Tipo` da categoria.
3. Categoria e Conta informadas devem existir.
4. Data não pode ser futura.
5. (Opcional) Saldo da conta = SaldoInicial + Σ receitas − Σ despesas (endpoint de consulta).

### Cuidados com PostgreSQL
- Npgsql grava `timestamp with time zone` e exige `DateTime` com `Kind = Utc` → converter para UTC no service.
- Definir precisão de `decimal` com `HasPrecision(18, 2)`.
- Enum salvo como `int` (padrão) ou string via `HasConversion<string>()`.
- (Opcional) `EFCore.NamingConventions` com `UseSnakeCaseNamingConvention()` para tabelas/colunas em snake_case.

---

## 5. Endpoints REST

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/api/transacoes` | Lista transações (filtros: `dataInicio`, `dataFim`, `categoriaId`, `contaId`, `tipo`; paginação: `page`, `pageSize`) | 200 |
| GET | `/api/transacoes/{id}` | Detalhe de uma transação | 200 / 404 |
| POST | `/api/transacoes` | Cria transação | 201 (Location) / 400 (ValidationProblemDetails) |
| GET | `/api/categorias` | Lista categorias | 200 |
| GET | `/api/contas` | Lista contas | 200 |
| GET | `/api/contas/{id}/saldo` | (Opcional) Saldo calculado | 200 / 404 |

Erros padronizados em **ProblemDetails** (RFC 7807) via middleware global de exceções.

---

## 6. Configuração e ambientes

### Connection string
`appsettings.Development.json` (dev local, API fora do container, banco no container):
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=fintrack;Username=fintrack;Password=fintrack"
}
```
No compose, sobrescrever por variável de ambiente:
`ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=fintrack;Username=fintrack;Password=fintrack`

> Credenciais reais nunca vão para o Git: usar `.env` (no `.gitignore`) e commitar só `.env.example`. Para dev local pode-se usar `dotnet user-secrets`.

### docker-compose.yml (visão geral)
- `db`: `postgres:16-alpine`, porta `5432:5432`, volume `pgdata`, `healthcheck` com `pg_isready`.
- `api`: build de `src/FinTrack.Api/Dockerfile` (contexto = raiz), porta `8080:8080`, `depends_on: db (service_healthy)`, `ASPNETCORE_ENVIRONMENT=Development` para expor o Swagger.

### Dockerfile (multi-stage)
1. `mcr.microsoft.com/dotnet/sdk:10.0` → `restore` (copiando os `.csproj` primeiro para cache) → `publish -c Release`.
2. `mcr.microsoft.com/dotnet/aspnet:10.0` → copia o publish, `EXPOSE 8080`, `ENTRYPOINT ["dotnet","FinTrack.Api.dll"]`.

### Migrations no container
Duas opções (escolher uma):
- **A (simples)**: `db.Database.Migrate()` no startup quando `Development`.
- **B**: rodar `dotnet ef database update` localmente apontando para `localhost:5432` antes de subir a API.

---

## 7. Fases de execução

| Fase | Entrega | Critério de pronto |
|---|---|---|
| 0. Repositório | Clone do GitHub, `.gitignore`, `.editorconfig`, README inicial | Primeiro commit/push no `main` |
| 1. Ambiente | .NET SDK, VS Code + extensões, Rancher Desktop, dotnet-ef | `dotnet --version`, `docker version`, `docker compose version` OK |
| 2. Solução | Solution + 4 projetos src + projeto de testes + referências | `dotnet build` sem erros |
| 3. Domínio | Entidades e enum | Compila; Domain sem dependências externas |
| 4. Persistência | DbContext, configurations, seed, repositórios, Postgres no compose, migration inicial | `dotnet ef database update` cria tabelas e seed no Postgres |
| 5. Aplicação | DTOs, validators, services, interfaces, DI | Regras de negócio implementadas |
| 6. API | Controllers, middleware de erro, Swagger, Program.cs | Endpoints funcionando via Swagger |
| 7. Testes | Testes de validator e service | `dotnet test` verde |
| 8. Containers | Dockerfile + compose completo | `docker compose up --build` sobe API + banco; Swagger acessível em `http://localhost:8080/swagger` |
| 9. Documentação | README com como rodar, endpoints e decisões | README revisado, push final |

Estratégia de Git: um branch por fase (`feat/fase-2-estrutura`, ...) com PR para `main`, ou commits diretos em `main` com mensagens no padrão Conventional Commits (`feat:`, `chore:`, `test:`, `docs:`).

---

## 8. Riscos e pontos de atenção

- **Rancher Desktop** precisa estar com o container engine **dockerd (moby)** (Preferences → Container Engine). Com `containerd` os comandos passam a ser `nerdctl`.
- Rancher Desktop no Windows usa **WSL 2**: habilitar antes da instalação.
- Porta 5432 pode já estar em uso se houver um PostgreSQL instalado no Windows → mudar para `5433:5432`.
- `DateTime` não-UTC causa exceção no Npgsql.
- Versões de pacotes EF Core / Npgsql devem ter o **mesmo major** do .NET usado.
- O `dotnet ef` precisa do pacote `Microsoft.EntityFrameworkCore.Design` no startup project (Api).
- Pasta de trabalho com espaço no nome (`Desafio .NET`) → sempre usar aspas nos caminhos do terminal.

---

## 9. Evoluções futuras (fora do escopo inicial)
- PUT/DELETE de transações, CRUD de categorias e contas.
- Autenticação JWT.
- Testes de integração com `WebApplicationFactory` + Testcontainers (PostgreSQL).
- CI no GitHub Actions (build + test).
- Logging estruturado (Serilog) e health checks (`/health`).
