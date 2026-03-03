# Controle de Gastos (Backend)

API em ASP.NET Core 8 para cadastro de pessoas, categorias, transacoes e consolidados.

## Pre-requisitos

- .NET SDK 8.0
- PostgreSQL 14+ (ou versao compativel)

## Configuracao local

1. Copie o arquivo de exemplo:
   - de `src/ControleGastos.API/appsettings.Development.example.json`
   - para `src/ControleGastos.API/appsettings.Development.json`
2. Preencha a senha real do banco no campo `Database:Password`.

Observacao: `src/ControleGastos.API/appsettings.Development.json` e ignorado pelo Git.

## Ordem de precedencia da connection string

No `Program.cs`, a resolucao segue esta ordem:

1. `ConnectionStrings__DefaultConnection` (variavel de ambiente)
2. `ConnectionStrings:DefaultConnection` (arquivos de configuracao)

Depois, se existir `Database:Password` (ou `DB_PASSWORD`), a senha e aplicada no `NpgsqlConnectionStringBuilder`.

## Variaveis de ambiente (alternativa ao appsettings local)

Exemplo com connection string completa:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=controle_gastos;Username=postgres;Password=postgres"
```

Exemplo com senha separada:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=controle_gastos;Username=postgres"
export DB_PASSWORD="postgres"
```

## Como executar

Na raiz `backend`:

```bash
dotnet run --project src/ControleGastos.API
```

## Como rodar testes

Na raiz `backend`:

```bash
dotnet test
```

## Nota de modelagem

O dominio permanece com `BirthDate` como fonte de verdade na entidade `Person`.
A idade (`Age`) continua sendo calculada dinamicamente.
