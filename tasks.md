# tasks — FinTrack API

> Checklist de execução. Detalhes e decisões em [PLAN.md](PLAN.md).
> Marque `[x]` conforme concluir. Comandos assumem terminal do VS Code (PowerShell) na **raiz do repositório**.

---

## Fase 0 — Repositório
- [x] Clonar o repositório: `git clone https://github.com/DaviLGS/FinTrackApi-.net.git`
- [x] Abrir a pasta clonada no VS Code (`code FinTrackApi-.net`)
- [x] Copiar `PLAN.md` e `tasks.md` para a raiz do repositório
- [x] Criar `.gitignore` do .NET: `dotnet new gitignore`
- [x] Adicionar `.env` ao `.gitignore`
- [x] Criar `.editorconfig`: `dotnet new editorconfig`
- [x] Criar `README.md` inicial (nome do projeto + descrição curta)
- [x] Commit `chore: setup inicial do repositório` e `git push`

## Fase 1 — Preparação do ambiente
- [x] Instalar .NET SDK (10 LTS, mínimo 8.0) e conferir: `dotnet --version`
- [ ] Instalar VS Code
- [ ] Instalar extensões: **C# Dev Kit**, **Container Tools/Docker** (Microsoft), **PostgreSQL** (Microsoft)
- [ ] Habilitar WSL 2 no Windows (`wsl --install`, se necessário)
- [ ] Instalar **Rancher Desktop**
- [ ] Em Preferences → Container Engine, selecionar **dockerd (moby)**
- [ ] Desabilitar Kubernetes no Rancher (opcional, economiza memória)
- [ ] Conferir: `docker version` e `docker compose version`
- [ ] Instalar ferramenta EF: `dotnet tool install --global dotnet-ef` e conferir `dotnet ef --version`

## Fase 2 — Estrutura da solução
- [x] Criar solução: `dotnet new sln -n FinTrack --format sln` (no .NET 10 o padrão seria `.slnx`)
- [x] Criar projetos:
  - [x] `dotnet new classlib -o src/FinTrack.Domain`
  - [x] `dotnet new classlib -o src/FinTrack.Application`
  - [x] `dotnet new classlib -o src/FinTrack.Infrastructure`
  - [x] `dotnet new webapi --use-controllers -o src/FinTrack.Api`
  - [x] `dotnet new xunit -o tests/FinTrack.Tests`
- [x] Adicionar à solução: `dotnet sln add (Get-ChildItem -Recurse -Filter *.csproj)` (ou um `dotnet sln add` por projeto)
- [x] Remover `Class1.cs` dos classlibs e o `WeatherForecast` de exemplo da Api
- [x] Configurar referências:
  - [x] `dotnet add src/FinTrack.Application reference src/FinTrack.Domain`
  - [x] `dotnet add src/FinTrack.Infrastructure reference src/FinTrack.Application src/FinTrack.Domain`
  - [x] `dotnet add src/FinTrack.Api reference src/FinTrack.Application src/FinTrack.Infrastructure`
  - [x] `dotnet add tests/FinTrack.Tests reference src/FinTrack.Domain src/FinTrack.Application`
- [x] Garantir mesmo `TargetFramework` e `<Nullable>enable</Nullable>` em todos os `.csproj`
- [x] Fixar `Microsoft.OpenApi` 2.13.0 na Api (a 2.0.0 trazida pelo template tem vulnerabilidade alta, GHSA-v5pm-xwqc-g5wc)
- [x] `dotnet build` sem erros
- [x] Commit `chore: estrutura da solução em clean architecture`

## Fase 3 — Domínio (FinTrack.Domain)
- [ ] Criar `Enums/TipoTransacao.cs` (`Receita = 1`, `Despesa = 2`)
- [ ] Criar `Entities/EntityBase.cs` (Id)
- [ ] Criar `Entities/Categoria.cs` (Nome, Tipo, coleção de Transacoes)
- [ ] Criar `Entities/Conta.cs` (Nome, SaldoInicial, coleção de Transacoes)
- [ ] Criar `Entities/Transacao.cs` (Descricao, Valor, Data, Tipo, CategoriaId, Categoria, ContaId, Conta, CriadoEm)
- [ ] Garantir que o Domain não tem nenhum pacote NuGet
- [ ] Commit `feat: entidades de domínio`

## Fase 4 — Persistência com EF Core + PostgreSQL (FinTrack.Infrastructure)
- [ ] Instalar pacotes na Infrastructure:
  - [ ] `dotnet add src/FinTrack.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL`
  - [ ] `dotnet add src/FinTrack.Infrastructure package Microsoft.EntityFrameworkCore.Tools`
  - [ ] (Opcional) `dotnet add src/FinTrack.Infrastructure package EFCore.NamingConventions`
- [ ] Instalar na Api: `dotnet add src/FinTrack.Api package Microsoft.EntityFrameworkCore.Design`
- [ ] Criar `Context/AppDbContext.cs` com `DbSet<Categoria>`, `DbSet<Conta>`, `DbSet<Transacao>` e `ApplyConfigurationsFromAssembly`
- [ ] Criar `Configurations/CategoriaConfiguration.cs` (tamanho, índice único em Nome, seed `HasData`)
- [ ] Criar `Configurations/ContaConfiguration.cs` (tamanho, `HasPrecision(18,2)`, seed `HasData`)
- [ ] Criar `Configurations/TransacaoConfiguration.cs` (tamanho, precisão, FKs com `OnDelete(Restrict)`, índices em Data/CategoriaId/ContaId)
- [ ] Criar interfaces de repositório na **Application** (`Interfaces/ITransacaoRepository.cs`, `ICategoriaRepository.cs`, `IContaRepository.cs`)
- [ ] Implementar `Repositories/TransacaoRepository.cs`, `CategoriaRepository.cs`, `ContaRepository.cs` (usar `AsNoTracking` nas leituras)
- [ ] Criar `DependencyInjection.cs` com `AddInfrastructure(IServiceCollection, IConfiguration)` registrando DbContext (`UseNpgsql`) e repositórios
- [ ] Criar `docker-compose.yml` inicial só com o serviço `db` (postgres:16-alpine, porta 5432, volume `pgdata`, healthcheck `pg_isready`)
- [ ] Criar `.env.example` com `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` e copiar para `.env`
- [ ] Subir o banco: `docker compose up -d db` e conferir `docker compose ps` (healthy)
- [ ] Adicionar `ConnectionStrings:DefaultConnection` no `appsettings.Development.json` (Host=localhost)
- [ ] Gerar migração: `dotnet ef migrations add InitialCreate --project src/FinTrack.Infrastructure --startup-project src/FinTrack.Api --output-dir Migrations`
- [ ] Aplicar: `dotnet ef database update --project src/FinTrack.Infrastructure --startup-project src/FinTrack.Api`
- [ ] Conferir tabelas e seed no Postgres (extensão PostgreSQL do VS Code ou `docker compose exec db psql -U fintrack -d fintrack -c "\dt"`)
- [ ] Commit `feat: persistência com ef core e postgresql`

## Fase 5 — Aplicação: regras de negócio e validações (FinTrack.Application)
- [ ] Instalar pacotes:
  - [ ] `dotnet add src/FinTrack.Application package FluentValidation`
  - [ ] `dotnet add src/FinTrack.Application package FluentValidation.DependencyInjectionExtensions`
- [ ] Criar `DTOs/TransacaoCreateDto.cs` (Descricao, Valor, Data, Tipo, CategoriaId, ContaId)
- [ ] Criar `DTOs/TransacaoResponseDto.cs` (inclui nome da categoria e da conta)
- [ ] Criar `DTOs/CategoriaDto.cs`, `DTOs/ContaDto.cs`
- [ ] Criar `DTOs/TransacaoFiltroDto.cs` (dataInicio, dataFim, categoriaId, contaId, tipo, page, pageSize) e `Common/PagedResult<T>`
- [ ] Criar `Validators/TransacaoCreateValidator.cs`:
  - [ ] Descricao obrigatória e ≤ 200
  - [ ] Valor > 0
  - [ ] Data obrigatória e não futura
  - [ ] Tipo é valor válido do enum (`IsInEnum`)
  - [ ] CategoriaId e ContaId > 0
- [ ] Criar exceções/resultado de aplicação (`Common/NotFoundException`, `Common/BusinessRuleException` ou `Result<T>`)
- [ ] Criar `Interfaces/ITransacaoService.cs` e `Services/TransacaoService.cs`:
  - [ ] `CriarAsync`: valida DTO, verifica existência de Categoria e Conta, verifica compatibilidade Tipo × Categoria.Tipo, converte Data para UTC, persiste e retorna DTO
  - [ ] `ObterPorIdAsync`
  - [ ] `ListarAsync(filtro)` com paginação
- [ ] Criar serviços simples de leitura para Categorias e Contas
- [ ] Criar `DependencyInjection.cs` com `AddApplication()` (services + `AddValidatorsFromAssemblyContaining<TransacaoCreateValidator>()`)
- [ ] Commit `feat: dtos, validações e serviços de aplicação`

## Fase 6 — API: controllers e endpoints REST (FinTrack.Api)
- [ ] Instalar Swagger: `dotnet add src/FinTrack.Api package Swashbuckle.AspNetCore`
- [ ] Configurar `Program.cs`:
  - [ ] `AddControllers()`
  - [ ] `AddApplication()` e `AddInfrastructure(builder.Configuration)`
  - [ ] `AddEndpointsApiExplorer()` + `AddSwaggerGen()`; `UseSwagger()` + `UseSwaggerUI()` em Development
  - [ ] `AddProblemDetails()` + middleware de exceção
  - [ ] (Opção A) aplicar migrations no startup em Development
- [ ] Criar `Middlewares/ExceptionHandlingMiddleware.cs` (ValidationException → 400, NotFound → 404, regra de negócio → 422/400, demais → 500 em ProblemDetails)
- [ ] Criar `Controllers/TransacoesController.cs`:
  - [ ] `GET /api/transacoes` com filtros/paginação
  - [ ] `GET /api/transacoes/{id}` → 200/404
  - [ ] `POST /api/transacoes` → 201 com `CreatedAtAction` / 400
  - [ ] Anotar `[ProducesResponseType]`
- [ ] Criar `Controllers/CategoriasController.cs` (`GET /api/categorias`)
- [ ] Criar `Controllers/ContasController.cs` (`GET /api/contas`, opcional `GET /api/contas/{id}/saldo`)
- [ ] Rodar `dotnet run --project src/FinTrack.Api` e testar no Swagger:
  - [ ] POST válido → 201
  - [ ] POST com valor negativo → 400
  - [ ] POST com categoria inexistente → 404/400
  - [ ] POST com tipo incompatível com a categoria → erro de regra
  - [ ] GET lista e GET por id
- [ ] (Opcional) Criar `FinTrack.Api.http` com requisições de exemplo
- [ ] Commit `feat: controllers e endpoints rest`

## Fase 7 — Testes unitários (FinTrack.Tests)
- [ ] Instalar: `dotnet add tests/FinTrack.Tests package NSubstitute`
- [ ] Criar `Validators/TransacaoCreateValidatorTests.cs`:
  - [ ] DTO válido → sem erros
  - [ ] Descricao vazia → erro
  - [ ] Descricao > 200 caracteres → erro
  - [ ] Valor 0 e negativo → erro (`[Theory]` + `[InlineData]`)
  - [ ] Data futura → erro
  - [ ] Tipo inválido → erro
  - [ ] CategoriaId/ContaId = 0 → erro
- [ ] Criar `Services/TransacaoServiceTests.cs` (repositórios mockados com NSubstitute):
  - [ ] Criação com sucesso chama `AddAsync` e retorna DTO
  - [ ] Categoria inexistente → exceção/erro
  - [ ] Conta inexistente → exceção/erro
  - [ ] Tipo incompatível com a categoria → exceção/erro
  - [ ] ObterPorId inexistente → NotFound
- [ ] Executar `dotnet test` e garantir tudo verde
- [ ] Commit `test: testes unitários de validator e service`

## Fase 8 — Containerização (Rancher Desktop)
- [ ] Criar `src/FinTrack.Api/Dockerfile` multi-stage (sdk → aspnet), copiando `.csproj` antes do código para cache de restore
- [ ] Criar `.dockerignore` na raiz (`bin/`, `obj/`, `.vs/`, `.vscode/`, `.git/`, `.env`)
- [ ] Completar `docker-compose.yml`:
  - [ ] Serviço `api` com `build: { context: ., dockerfile: src/FinTrack.Api/Dockerfile }`
  - [ ] Porta `8080:8080`
  - [ ] `ConnectionStrings__DefaultConnection` apontando para `Host=db`
  - [ ] `ASPNETCORE_ENVIRONMENT=Development`
  - [ ] `depends_on: db: condition: service_healthy`
- [ ] Subir tudo: `docker compose up --build`
- [ ] Acessar `http://localhost:8080/swagger` e repetir os testes da Fase 6
- [ ] Verificar persistência: `docker compose down` + `docker compose up` mantém os dados (volume)
- [ ] Commit `feat: dockerfile e docker-compose`

## Fase 9 — Documentação e entrega
- [ ] Completar `README.md`: descrição, arquitetura, pré-requisitos (.NET, Rancher Desktop), como rodar local e via compose, como rodar migrations e testes, lista de endpoints
- [ ] Revisar `PLAN.md` / `tasks.md` (marcar o que foi feito)
- [ ] Conferir que nenhum segredo (`.env`) foi commitado
- [ ] `dotnet build` + `dotnet test` finais
- [ ] Commit `docs: readme` e `git push`

## Extras (opcional)
- [ ] PUT/DELETE de transações
- [ ] Endpoint de saldo por conta
- [ ] Health check `/health` (incluindo Postgres)
- [ ] GitHub Actions: build + test em push/PR
- [ ] Testes de integração com `WebApplicationFactory` + Testcontainers
- [ ] Logging com Serilog
