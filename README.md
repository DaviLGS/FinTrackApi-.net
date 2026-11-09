# FinTrack API

API REST de controle financeiro pessoal: registra e consulta **transações** (receitas e despesas), vinculadas a **contas** e **categorias**.

Feita em **.NET 10** com **Clean Architecture**, **EF Core + PostgreSQL**, **FluentValidation**, **Swagger**, **Serilog** e testes com **xUnit** e **Testcontainers**. Os containers rodam no **Rancher Desktop**.

O plano do projeto está em [PLAN.md](PLAN.md) e o andamento em [tasks.md](tasks.md).

## Pré-requisitos

- [Rancher Desktop](https://rancherdesktop.io/) com o container engine **dockerd (moby)**, em Preferences → Container Engine. Ele precisa estar aberto.
- [.NET SDK 10](https://dotnet.microsoft.com/download), para rodar a API fora do container, os testes e as migrations.

## Como rodar

### Tudo em containers

A partir da raiz do repositório:

```bash
cp .env.example .env
docker compose up --build
```

- Swagger: <http://localhost:8080/swagger>
- API: <http://localhost:8080/api/transacoes>

As migrations e os dados iniciais são aplicados quando a API inicia.

Para parar:

```bash
docker compose down
```

Os dados ficam no volume `pgdata` e continuam lá depois do `down`. Para apagar o banco também, use `docker compose down -v`.

### API local e banco no container

Melhor para desenvolver, porque a API roda direto na máquina:

```bash
cp .env.example .env
docker compose up -d db
dotnet run --project src/FinTrack.Api --launch-profile http
```

- Swagger: <http://localhost:5215/swagger>

A connection string desse modo está em `src/FinTrack.Api/appsettings.Development.json`. Se mudar a porta ou a senha no `.env`, atualize esse arquivo também.

O arquivo [FinTrack.Api.http](src/FinTrack.Api/FinTrack.Api.http) tem exemplos de requisições.

### Portas

| Variável no `.env` | Padrão | Uso |
|---|---|---|
| `POSTGRES_PORT` | `5433` | Postgres no host. Não é a 5432 para não conflitar com um PostgreSQL instalado na máquina. |
| `API_PORT` | `8080` | API no container. |

## Testes

Testes unitários dos validadores e dos serviços, com os repositórios simulados. Não precisam de banco nem de Docker:

```bash
dotnet test tests/FinTrack.Tests
```

Testes de integração, que chamam a API em memória ligada a um PostgreSQL real. O Testcontainers cria e remove um container descartável, então o Rancher Desktop precisa estar aberto, mas o banco de desenvolvimento não é tocado:

```bash
dotnet test tests/FinTrack.IntegrationTests
```

Para rodar tudo, use `dotnet test`. O GitHub Actions ([ci.yml](.github/workflows/ci.yml)) faz o build, roda todos os testes e monta a imagem Docker a cada push ou pull request na `main`.

## Migrations

O `dotnet-ef` é uma ferramenta local do repositório, declarada em `dotnet-tools.json`:

```bash
dotnet tool restore
```

Para criar uma migration:

```bash
dotnet ef migrations add NomeDaMigration --project src/FinTrack.Infrastructure --startup-project src/FinTrack.Api --output-dir Migrations
```

Para aplicar manualmente, com o banco no ar:

```bash
dotnet ef database update --project src/FinTrack.Infrastructure --startup-project src/FinTrack.Api
```

Em ambiente `Development`, a API já aplica as migrations pendentes ao iniciar.

Na primeira execução com o banco vazio, o log mostra uma linha `fail` para `SELECT ... FROM "__EFMigrationsHistory"`. É esperado: o EF Core consulta essa tabela antes de ela existir e, em seguida, aplica as migrations normalmente.

## Endpoints

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/api/transacoes` | Lista transações, das mais recentes para as mais antigas | 200, 400 |
| GET | `/api/transacoes/{id}` | Busca uma transação | 200, 404 |
| POST | `/api/transacoes` | Cria uma transação | 201, 400, 422 |
| PUT | `/api/transacoes/{id}` | Substitui os dados de uma transação | 200, 400, 404, 422 |
| DELETE | `/api/transacoes/{id}` | Remove uma transação | 204, 404 |
| GET | `/api/categorias` | Lista as categorias | 200 |
| GET | `/api/contas` | Lista as contas | 200 |
| GET | `/api/contas/{id}/saldo` | Calcula o saldo: saldo inicial + receitas - despesas | 200, 404 |
| GET | `/health` | Verifica a API e a conexão com o banco | 200, 503 |

Filtros opcionais de `GET /api/transacoes`: `dataInicio`, `dataFim`, `categoriaId`, `contaId`, `tipo`, `page` (padrão 1) e `pageSize` (padrão 20, máximo 100).

### Criar uma transação

```http
POST /api/transacoes
Content-Type: application/json

{
  "descricao": "Almoço",
  "valor": 42.90,
  "data": "2026-10-20T12:30:00Z",
  "tipo": "Despesa",
  "categoriaId": 3,
  "contaId": 1
}
```

O `PUT` usa o mesmo corpo e as mesmas regras. Ele não altera `criadoEm`.

### Regras

- `descricao` é obrigatória, com até 200 caracteres.
- `valor` deve ser maior que zero. Quem indica se é entrada ou saída é o `tipo`.
- `data` é obrigatória e não pode ser futura. Uma data sem fuso horário é tratada como UTC.
- `tipo` é `Receita` ou `Despesa` e precisa ser igual ao tipo da categoria.
- `categoriaId` e `contaId` precisam existir.

### Erros

Os erros seguem o formato ProblemDetails (RFC 7807):

| Status | Quando |
|---|---|
| 400 | Campos ou filtros inválidos. A resposta lista os erros por campo. |
| 404 | A transação ou a conta da rota não existe. |
| 422 | Regra de negócio violada: categoria ou conta inexistente, ou tipo incompatível com a categoria. |
| 500 | Erro inesperado. Os detalhes ficam só no log. |

### Dados iniciais

| Id | Categoria | Tipo |
|---|---|---|
| 1 | Salário | Receita |
| 2 | Freelance | Receita |
| 3 | Alimentação | Despesa |
| 4 | Transporte | Despesa |
| 5 | Moradia | Despesa |
| 6 | Lazer | Despesa |

Contas: `1` Carteira e `2` Conta Corrente.

## Arquitetura

As dependências apontam para dentro, e o Domain não depende de nenhuma outra camada.

| Projeto | Responsabilidade | Referencia |
|---|---|---|
| `FinTrack.Domain` | Entidades e enums | — |
| `FinTrack.Application` | DTOs, validações, serviços e interfaces dos repositórios | Domain |
| `FinTrack.Infrastructure` | EF Core, `AppDbContext`, repositórios e migrations | Application, Domain |
| `FinTrack.Api` | Controllers, middleware de erros, Swagger e `Program.cs` | Application, Infrastructure |
| `FinTrack.Tests` | Testes unitários | Application, Domain |
| `FinTrack.IntegrationTests` | Testes da API com PostgreSQL real | Api, Application |

## Logs

A API usa o Serilog e escreve no console, com uma linha por requisição. Os níveis ficam na seção `Serilog` do `appsettings.json`.

## Limitações atuais

- Não há autenticação.
- Categorias e contas são somente leitura e vêm dos dados iniciais.
- O compose roda a API em `Development`, para ter Swagger e migrations automáticas. Ele serve para desenvolvimento, não para produção.
- Fora de `Development`, não há Swagger nem migrations automáticas, e a connection string precisa vir de configuração (por exemplo, a variável `ConnectionStrings__DefaultConnection`).

As próximas evoluções estão nos extras do [tasks.md](tasks.md).
