# FinTrack API

API REST de controle financeiro pessoal: cada usuário se cadastra com e-mail e senha, faz login e recebe um **token JWT**. Com ele, registra e consulta as próprias **transações** (receitas e despesas), vinculadas a **contas** e **categorias**, e gera **relatórios** mensais e anuais.

Feita em **.NET 10** com **Clean Architecture**, **EF Core + PostgreSQL**, **FluentValidation**, **Swagger**, **Serilog** e testes com **xUnit** e **Testcontainers**. Os containers rodam no **Rancher Desktop**.

O plano do projeto está em [PLAN.md](PLAN.md) e o andamento em [tasks.md](tasks.md).

## Pré-requisitos

- [Rancher Desktop](https://rancherdesktop.io/) com o container engine **dockerd (moby)**, em Preferences → Container Engine. Ele precisa estar aberto.
- [.NET SDK 10](https://dotnet.microsoft.com/download), para rodar a API fora do container, os testes e as migrations.
- [Node.js 20+](https://nodejs.org/), só para o front de teste em `web/`.

## Como rodar

### Tudo em containers

A partir da raiz do repositório:

```bash
cp .env.example .env
docker compose up --build
```

- Swagger: <http://localhost:8080/swagger>
- API: <http://localhost:8080/api/transacoes> (exige token; veja [Autenticação](#autenticação))

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

O `.env` também define `JWT_KEY`, a chave que assina os tokens (mínimo de 32 caracteres). Fora do desenvolvimento, use um valor aleatório. Ao rodar a API fora do container, a chave vem do `appsettings.Development.json`.

## Front de teste (React)

Em `web/` há um front simples, feito com **Vite + React + TypeScript**, só para testar a API pelo navegador:

- **Cadastro** e **login** (o token fica no `localStorage`; ao expirar, volta para o login).
- **Início**: relatório mensal ou anual, com entradas, gastos e saldo, gastos e entradas por categoria e, no anual, o resumo mês a mês.
- **Lançamentos**: saldo de cada conta, formulário para adicionar ou editar gastos e entradas, e o histórico com filtro, paginação e exclusão.

Com a API rodando na porta 8080 (por exemplo, com `docker compose up -d`):

```bash
cd web
npm install
npm run dev
```

Abra http://localhost:5173. O Vite repassa as chamadas `/api` para `http://localhost:8080`, então não é preciso configurar CORS. Para usar outra porta da API, defina `API_URL` antes do `npm run dev` (por exemplo, `API_URL=http://localhost:5000`).

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

A migration `AdicionarUsuarios` apaga as transações que existiam antes do cadastro de usuários, porque elas não têm dono.

Na primeira execução com o banco vazio, o log mostra uma linha `fail` para `SELECT ... FROM "__EFMigrationsHistory"`. É esperado: o EF Core consulta essa tabela antes de ela existir e, em seguida, aplica as migrations normalmente.

## Autenticação

1. Cadastre um usuário com `POST /api/auth/registrar`.
2. Faça login com `POST /api/auth/login` usando o mesmo e-mail e senha. A resposta traz o `accessToken`, válido por 60 minutos.
3. Envie o token nas demais rotas, no header `Authorization: Bearer <accessToken>`. No Swagger, use o botão **Authorize**.

```http
POST /api/auth/registrar
Content-Type: application/json

{ "email": "davi@exemplo.com", "senha": "senha-forte-123" }
```

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "davi@exemplo.com", "senha": "senha-forte-123" }
```

```json
{ "accessToken": "eyJhbGciOi...", "tokenType": "Bearer", "expiraEm": "2026-11-09T14:02:32Z" }
```

- O e-mail é guardado sem espaços e em minúsculas, então `Davi@Exemplo.com` e `davi@exemplo.com` são o mesmo usuário.
- A senha precisa ter de 8 a 128 caracteres. Só o hash dela é salvo (PBKDF2, via `PasswordHasher` do ASP.NET Core Identity).
- E-mail inexistente e senha errada dão a mesma resposta (401), para não revelar quem tem cadastro.
- Cada usuário só enxerga, altera e soma as próprias transações. Transações de outro usuário respondem 404. Categorias e contas são compartilhadas.

## Endpoints

Todas as rotas exigem token, exceto `/api/auth/*` e `/health`. Sem token válido, a resposta é 401.

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| POST | `/api/auth/registrar` | Cadastra um usuário com e-mail e senha | 201, 400, 409 |
| POST | `/api/auth/login` | Faz login e retorna o token JWT | 200, 400, 401 |
| GET | `/api/transacoes` | Lista transações, das mais recentes para as mais antigas | 200, 400 |
| GET | `/api/transacoes/{id}` | Busca uma transação | 200, 404 |
| POST | `/api/transacoes` | Cria uma transação | 201, 400, 422 |
| PUT | `/api/transacoes/{id}` | Substitui os dados de uma transação | 200, 400, 404, 422 |
| DELETE | `/api/transacoes/{id}` | Remove uma transação | 204, 404 |
| GET | `/api/categorias` | Lista as categorias | 200 |
| GET | `/api/contas` | Lista as contas | 200 |
| GET | `/api/contas/{id}/saldo` | Calcula o saldo: saldo inicial + receitas - despesas | 200, 404 |
| GET | `/api/relatorios/mensal?ano=2026&mes=10` | Gastos e entradas por categoria no mês | 200, 400 |
| GET | `/api/relatorios/anual?ano=2026` | Gastos e entradas por categoria no ano, com o resumo de cada mês | 200, 400 |
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

### Relatórios

O relatório mensal traz, para o mês pedido, o total de receitas, o de despesas e o saldo, além das listas `despesasPorCategoria` e `receitasPorCategoria`. Cada item tem o total da categoria e o percentual sobre o total do mesmo tipo, do maior para o menor.

O anual traz o mesmo para o ano inteiro e a lista `meses`, sempre com os 12 meses: receitas, despesas e saldo de cada um, com zero nos meses sem movimentação.

```json
{
  "ano": 2026, "mes": 10,
  "totalReceitas": 5000.00, "totalDespesas": 500.00, "saldo": 4500.00,
  "despesasPorCategoria": [
    { "categoriaId": 3, "categoria": "Alimentação", "total": 400.00, "percentual": 80 },
    { "categoriaId": 4, "categoria": "Transporte", "total": 100.00, "percentual": 20 }
  ],
  "receitasPorCategoria": [
    { "categoriaId": 1, "categoria": "Salário", "total": 5000.00, "percentual": 100 }
  ]
}
```

Os meses são calculados em UTC: uma transação às 22h do dia 31 no horário de Brasília (UTC-3) já conta no mês seguinte.

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
| 401 | Token ausente, inválido ou expirado, ou e-mail e senha incorretos no login. |
| 404 | A transação ou a conta da rota não existe, ou a transação é de outro usuário. |
| 409 | Já existe um usuário com o e-mail informado. |
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
| `FinTrack.Domain` | Entidades (`Usuario`, `Transacao`, `Categoria`, `Conta`) e enums | — |
| `FinTrack.Application` | DTOs, validações, serviços (transações, autenticação, relatórios) e interfaces de repositórios, hash de senha, token e usuário atual | Domain |
| `FinTrack.Infrastructure` | EF Core, `AppDbContext`, repositórios, migrations, hash de senha e geração do JWT | Application, Domain |
| `FinTrack.Api` | Controllers, validação do JWT, usuário atual, middleware de erros, Swagger e `Program.cs` | Application, Infrastructure |
| `FinTrack.Tests` | Testes unitários | Application, Domain, Infrastructure |
| `FinTrack.IntegrationTests` | Testes da API com PostgreSQL real | Api, Application |
| `web/` | Front de teste em React, fora da solution .NET | — (consome a API por HTTP) |

## Logs

A API usa o Serilog e escreve no console, com uma linha por requisição. Os níveis ficam na seção `Serilog` do `appsettings.json`.

## Limitações atuais

- Não há troca nem recuperação de senha, nem refresh token: quando o token expira, é preciso fazer login de novo.
- Categorias e contas são somente leitura e vêm dos dados iniciais.
- O front em `web/` é só para testes: não tem testes automatizados nem build para produção no compose.
- O compose roda a API em `Development`, para ter Swagger e migrations automáticas. Ele serve para desenvolvimento, não para produção.
- Fora de `Development`, não há Swagger nem migrations automáticas, e a connection string precisa vir de configuração (por exemplo, a variável `ConnectionStrings__DefaultConnection`).

As próximas evoluções estão nos extras do [tasks.md](tasks.md).
