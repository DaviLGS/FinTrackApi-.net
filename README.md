# FinTrack API

API REST de controle financeiro pessoal (transações, contas e categorias) construída em **.NET 10** com **Clean Architecture**, **EF Core + PostgreSQL**, **FluentValidation**, **Swagger**, testes com **xUnit** e execução em containers via **Rancher Desktop**.

> Projeto em construção. Veja o [PLAN.md](PLAN.md) para o plano completo e o [tasks.md](tasks.md) para o andamento.

## Estrutura

```
src/
  FinTrack.Domain/          Entidades e enums
  FinTrack.Application/     DTOs, validações, serviços e interfaces
  FinTrack.Infrastructure/  EF Core, DbContext, repositórios e migrations
  FinTrack.Api/             Controllers, middlewares e Swagger
tests/
  FinTrack.Tests/           Testes unitários (xUnit)
```

## Build

```bash
dotnet build
```
