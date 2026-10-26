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
- [x] Instalar VS Code
- [ ] Instalar extensões: **C# Dev Kit**, **Container Tools/Docker** (Microsoft), **PostgreSQL** (Microsoft)
- [ ] Habilitar WSL 2 no Windows (`wsl --install`, se necessário)
- [x] Instalar **Rancher Desktop**
- [ ] Em Preferences → Container Engine, selecionar **dockerd (moby)**
- [ ] Desabilitar Kubernetes no Rancher (opcional, economiza memória)
- [x] Conferir: `docker version` e `docker compose version`
- [x] Instalar ferramenta EF como tool **local** (manifesto `dotnet-tools.json` versionado): `dotnet new tool-manifest` + `dotnet tool install dotnet-ef` — em outra máquina basta `dotnet tool restore`

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
- [x] Criar `Enums/TipoTransacao.cs` (`Receita = 1`, `Despesa = 2`)
- [x] Criar `Entities/EntityBase.cs` (Id)
- [x] Criar `Entities/Categoria.cs` (Nome, Tipo, coleção de Transacoes)
- [x] Criar `Entities/Conta.cs` (Nome, SaldoInicial, coleção de Transacoes)
- [x] Criar `Entities/Transacao.cs` (Descricao, Valor, Data, Tipo, CategoriaId, Categoria, ContaId, Conta, CriadoEm)
- [x] Garantir que o Domain não tem nenhum pacote NuGet
- [x] Commit `feat: entidades de domínio`

## Fase 4 — Persistência com EF Core + PostgreSQL (FinTrack.Infrastructure)
- [x] Instalar pacotes na Infrastructure:
  - [x] `dotnet add src/FinTrack.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL`
  - [x] `dotnet add src/FinTrack.Infrastructure package Microsoft.EntityFrameworkCore.Tools`
  - [x] (Opcional) `dotnet add src/FinTrack.Infrastructure package EFCore.NamingConventions` (tabelas/colunas em snake_case)
- [x] Instalar na Api: `dotnet add src/FinTrack.Api package Microsoft.EntityFrameworkCore.Design`
- [x] Criar `Context/AppDbContext.cs` com `DbSet<Categoria>`, `DbSet<Conta>`, `DbSet<Transacao>` e `ApplyConfigurationsFromAssembly`
- [x] Criar `Configurations/CategoriaConfiguration.cs` (tamanho, índice único em Nome, seed `HasData`)
- [x] Criar `Configurations/ContaConfiguration.cs` (tamanho, `HasPrecision(18,2)`, seed `HasData`)
- [x] Criar `Configurations/TransacaoConfiguration.cs` (tamanho, precisão, FKs com `OnDelete(Restrict)`, índices em Data/CategoriaId/ContaId)
- [x] Criar interfaces de repositório na **Application** (`Interfaces/ITransacaoRepository.cs`, `ICategoriaRepository.cs`, `IContaRepository.cs`)
- [x] Implementar `Repositories/TransacaoRepository.cs`, `CategoriaRepository.cs`, `ContaRepository.cs` (usar `AsNoTracking` nas leituras)
- [x] Criar `DependencyInjection.cs` com `AddInfrastructure(IServiceCollection, IConfiguration)` registrando DbContext (`UseNpgsql`) e repositórios
- [x] Chamar `AddInfrastructure` no `Program.cs` (necessário para o `dotnet ef` encontrar o DbContext)
- [x] Criar `docker-compose.yml` inicial só com o serviço `db` (postgres:16-alpine, porta do host **5433** (já existe um PostgreSQL local na 5432), volume `pgdata`, healthcheck `pg_isready`)
- [x] Criar `.env.example` com `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT` e copiar para `.env`
- [x] Subir o banco: `docker compose up -d db` e conferir `docker compose ps` (healthy)
- [x] Adicionar `ConnectionStrings:DefaultConnection` no `appsettings.Development.json` (Host=localhost;Port=5433)
- [x] Gerar migração: `dotnet ef migrations add InitialCreate --project src/FinTrack.Infrastructure --startup-project src/FinTrack.Api --output-dir Migrations`
- [x] Aplicar: `dotnet ef database update --project src/FinTrack.Infrastructure --startup-project src/FinTrack.Api`
- [x] Conferir tabelas e seed no Postgres (extensão PostgreSQL do VS Code ou `docker compose exec db psql -U fintrack -d fintrack -c "\dt"`)
- [x] Commit `feat: persistência com ef core e postgresql`

## Fase 5 — Aplicação: regras de negócio e validações (FinTrack.Application)
- [x] Instalar pacotes:
  - [x] `dotnet add src/FinTrack.Application package FluentValidation`
  - [x] `dotnet add src/FinTrack.Application package FluentValidation.DependencyInjectionExtensions`
- [x] Criar `DTOs/TransacaoCreateDto.cs` (Descricao, Valor, Data, Tipo, CategoriaId, ContaId)
- [x] Criar `DTOs/TransacaoResponseDto.cs` (inclui nome da categoria e da conta)
- [x] Criar `DTOs/CategoriaDto.cs`, `DTOs/ContaDto.cs`
- [x] Criar `DTOs/TransacaoFiltroDto.cs` (dataInicio, dataFim, categoriaId, contaId, tipo, page, pageSize) e `Common/PagedResult<T>` (feito na Fase 4, usado pelo repositório)
- [x] Criar `Validators/TransacaoCreateValidator.cs`:
  - [x] Descricao obrigatória e ≤ 200
  - [x] Valor > 0
  - [x] Data obrigatória e não futura (via `TimeProvider` injetado, para facilitar os testes)
  - [x] Tipo é valor válido do enum (`IsInEnum`)
  - [x] CategoriaId e ContaId > 0
- [x] Criar exceções de aplicação (`Common/Exceptions/NotFoundException` → 404, `Common/Exceptions/BusinessRuleException` → 422)
- [x] Criar `Common/DateTimeExtensions.ParaUtc()` (reaproveitado no repositório) e `Mappings/MappingExtensions` (entidade → DTO)
- [x] Criar `Interfaces/ITransacaoService.cs` e `Services/TransacaoService.cs`:
  - [x] `CriarAsync`: valida DTO, verifica existência de Categoria e Conta (inexistente → `BusinessRuleException`), verifica compatibilidade Tipo × Categoria.Tipo, converte Data para UTC, persiste e retorna DTO
  - [x] `ObterPorIdAsync`
  - [x] `ListarAsync(filtro)` com paginação
- [x] Criar `Validators/TransacaoFiltroValidator.cs` (page > 0, pageSize 1..100, dataFim ≥ dataInicio, tipo válido)
- [x] Criar serviços simples de leitura para Categorias e Contas
- [x] Criar `DependencyInjection.cs` com `AddApplication()` (services + `AddValidatorsFromAssemblyContaining<TransacaoCreateValidator>()` + `TimeProvider.System`)
- [x] Chamar `AddApplication()` no `Program.cs`
- [x] Commit `feat: dtos, validações e serviços de aplicação`

## Fase 6 — API: controllers e endpoints REST (FinTrack.Api)
- [x] Instalar Swagger: `dotnet add src/FinTrack.Api package Swashbuckle.AspNetCore` (10.3.1)
- [x] Remover `Microsoft.AspNetCore.OpenApi` do template e o pin de `Microsoft.OpenApi` (o Swashbuckle já traz a 2.7.5, sem vulnerabilidade)
- [x] Habilitar comentários XML (`GenerateDocumentationFile`) para os `///` aparecerem no Swagger
- [x] Configurar `Program.cs`:
  - [x] `AddControllers()` + `JsonStringEnumConverter` (enums como "Receita"/"Despesa")
  - [x] `AddRouting(LowercaseUrls = true)` (rotas `/api/transacoes` em minúsculas)
  - [x] `AddApplication()` e `AddInfrastructure(builder.Configuration)`
  - [x] `AddEndpointsApiExplorer()` + `AddSwaggerGen()`; `UseSwagger()` + `UseSwaggerUI()` em Development
  - [x] `AddProblemDetails()` + middleware de exceção
  - [x] (Opção A) aplicar migrations no startup em Development (`DatabaseExtensions.AplicarMigrationsAsync`)
  - [x] `UseHttpsRedirection()` só fora de Development (no container a API roda em HTTP)
- [x] Criar `Middlewares/ExceptionHandlingMiddleware.cs` (ValidationException → 400 com erros por campo em camelCase, NotFound → 404, regra de negócio → 422, demais → 500 em ProblemDetails)
- [x] Criar `Controllers/TransacoesController.cs`:
  - [x] `GET /api/transacoes` com filtros/paginação
  - [x] `GET /api/transacoes/{id}` → 200/404
  - [x] `POST /api/transacoes` → 201 com `CreatedAtAction` / 400 / 422
  - [x] Anotar `[ProducesResponseType]`
- [x] Criar `Controllers/CategoriasController.cs` (`GET /api/categorias`)
- [x] Criar `Controllers/ContasController.cs` (`GET /api/contas`; o endpoint de saldo ficou nos Extras)
- [x] Rodar `dotnet run --project src/FinTrack.Api` e testar (curl + Swagger em `http://localhost:5215/swagger`):
  - [x] POST válido → 201 (com header `Location`)
  - [x] POST com valor negativo → 400
  - [x] POST com tipo inexistente (`"Banana"`) → 400
  - [x] Filtro inválido (dataFim < dataInicio, pageSize > 100) → 400
  - [x] POST com categoria inexistente → 422
  - [x] POST com tipo incompatível com a categoria → 422
  - [x] GET lista (com filtros por tipo e período) e GET por id (200 e 404)
- [x] (Opcional) Criar `FinTrack.Api.http` com requisições de exemplo
- [x] Commit `feat: controllers e endpoints rest`

## Fase 7 — Testes unitários (FinTrack.Tests)
- [x] Instalar: `dotnet add tests/FinTrack.Tests package NSubstitute`
- [x] Instalar: `dotnet add tests/FinTrack.Tests package Microsoft.Extensions.TimeProvider.Testing` (`FakeTimeProvider` fixa o "agora" nos testes de data futura)
- [x] Criar `Validators/TransacaoCreateValidatorTests.cs`:
  - [x] DTO válido → sem erros
  - [x] Descricao vazia → erro
  - [x] Descricao > 200 caracteres → erro (e exatamente 200 → válido)
  - [x] Valor 0 e negativo → erro (`[Theory]` + `[InlineData]`)
  - [x] Data futura → erro (data igual ao momento atual → válida; data não informada → erro)
  - [x] Tipo inválido → erro
  - [x] CategoriaId/ContaId = 0 → erro
- [x] Criar `Validators/TransacaoFiltroValidatorTests.cs` (filtro padrão válido, page 0, pageSize fora de 1..100, dataFim < dataInicio, tipo inválido)
- [x] Criar `Services/TransacaoServiceTests.cs` (repositórios mockados com NSubstitute):
  - [x] Criação com sucesso chama `AdicionarAsync` e retorna DTO
  - [x] Categoria inexistente → `BusinessRuleException`
  - [x] Conta inexistente → `BusinessRuleException`
  - [x] Tipo incompatível com a categoria → `BusinessRuleException`
  - [x] ObterPorId inexistente → `NotFoundException` (e existente → DTO mapeado)
  - [x] DTO inválido → `ValidationException` sem persistir
  - [x] Data sem fuso convertida para UTC; descrição com espaços é aparada
  - [x] ListarAsync: filtro inválido → `ValidationException`; resultado paginado mapeado
- [x] Executar `dotnet test` e garantir tudo verde (33 testes aprovados)
- [x] Commit `test: testes unitários de validator e service`

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
- [ ] Antes de um CRUD de categorias/contas: ajustar as sequences de identity após o seed (`HasData` usa Ids fixos 1..N e a sequence começa em 1)
- [ ] Endpoint de saldo por conta
- [ ] Health check `/health` (incluindo Postgres)
- [ ] GitHub Actions: build + test em push/PR
- [ ] Testes de integração com `WebApplicationFactory` + Testcontainers
- [ ] Logging com Serilog
