# Controle de Gastos - Backend

## Decisoes de regra de negocio

- **Pessoa**: o cadastro recebe `BirthDate` (data de nascimento), nao `Age`. A idade e calculada dinamicamente a partir da data. Isso evita desatualizacao com o passar do tempo e garante que as regras de menoridade sejam sempre aplicadas corretamente.
- **Transacao**:
  - valor deve ser positivo;
  - pessoa e categoria devem existir;
  - menor de idade so pode registrar despesa;
  - categoria deve ser compativel com o tipo da transacao (despesa/receita/ambas).
- **Totais**: as consultas de totais por pessoa e por categoria retornam itens e um resumo geral (totais e saldo liquido) conforme o enunciado do teste.

## Observacoes de implementacao

- As entidades de dominio possuem validacoes basicas (ex.: nome obrigatorio, data de nascimento nao futura, descricao limitada).
- Os endpoints retornam DTOs de resposta para evitar exposicao de entidades e ciclos de serializacao.

## Como executar (backend)

### Pre-requisitos

- .NET SDK 8
- PostgreSQL

### Configuracao

1. Ajuste a connection string em `src/ControleGastos.API/appsettings.json`:
   - `ConnectionStrings:DefaultConnection`
2. Garanta que o banco exista e esteja acessivel.

### Migrations

Execute as migrations a partir do projeto da API:

```bash
dotnet ef database update --project src/ControleGastos.Infrastructure --startup-project src/ControleGastos.API
```

### Executar a API

```bash
dotnet run --project src/ControleGastos.API
```

A API sobe em HTTPS/HTTP conforme `src/ControleGastos.API/Properties/launchSettings.json`.
O Swagger estara disponivel na URL base com `/swagger`.
