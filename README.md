# Controle de Gastos Residenciais

Sistema full stack para controle de gastos, separado em **Web API (.NET)** e **Frontend (React + TypeScript)**, conforme os requisitos do teste técnico.

## Objetivo

Permitir o gerenciamento de:

- Pessoas
- Categorias
- Transações
- Consultas de totais

Com aplicação das regras de negócio do domínio financeiro residencial.

## Arquitetura

- `backend/`: API REST em C#/.NET com EF Core e PostgreSQL.
- `frontend/controle-gastos-web/`: aplicação React (Vite + TypeScript).

Estrutura macro:

```text
controle-gastos/
  backend/
    src/
      ControleGastos.API/
      ControleGastos.Application/
      ControleGastos.Domain/
      ControleGastos.Infrastructure/
  frontend/
    controle-gastos-web/
```

## Tecnologias

### Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- FluentValidation

### Frontend

- React 19
- TypeScript
- Vite
- Axios
- React Router

## Regras de negócio implementadas

- Pessoa:
  - identificação automática (`Guid`)
  - nome obrigatório, máximo de 200 caracteres
  - uso de `BirthDate` para cálculo de idade dinâmico
  - exclusão em cascata das transações ao remover pessoa

- Categoria:
  - identificação automática (`Guid`)
  - descrição obrigatória, máximo de 400 caracteres
  - finalidade: despesa, receita ou ambas

- Transação:
  - identificação automática (`Guid`)
  - descrição obrigatória, máximo de 400 caracteres
  - valor > 0
  - pessoa e categoria obrigatórias e válidas
  - menor de idade só pode registrar despesa
  - categoria precisa ser compatível com o tipo da transação

- Totais:
  - totais por pessoa e resumo geral
  - totais por categoria e resumo geral

## Funcionalidades do frontend

- Transações (página principal): criação e listagem
- Pessoas: criação, listagem, edição e exclusão
- Categorias: criação e listagem

Padrões adotados:

- métodos e propriedades técnicas em inglês (alinhado ao backend)
- textos exibidos ao usuário em português

## Como executar o projeto

## Pré-requisitos

- .NET SDK 8
- Node.js 20+
- PostgreSQL

## 1) Subir backend

1. Configure a connection string em:
   - `backend/src/ControleGastos.API/appsettings.json`
   - chave: `ConnectionStrings:DefaultConnection`

2. Aplique as migrations:

```bash
cd backend
dotnet ef database update --project src/ControleGastos.Infrastructure --startup-project src/ControleGastos.API
```

3. Execute a API:

```bash
dotnet run --project src/ControleGastos.API
```

A API roda nas URLs do `launchSettings.json` (ex.: `http://localhost:5034`).
Swagger disponível em `/swagger`.

## 2) Subir frontend

1. Instale dependências:

```bash
cd frontend/controle-gastos-web
npm install
```

2. Configure a URL da API (opcional). Padrão: `http://localhost:5034/api`.

Crie um `.env` em `frontend/controle-gastos-web`:

```env
VITE_API_BASE_URL=http://localhost:5034/api
```

3. Execute:

```bash
npm run dev
```

## Scripts úteis (frontend)

```bash
npm run lint
npm run build
```

## Endpoints principais

- `POST /api/person`
- `GET /api/person`
- `PUT /api/person/{id}`
- `DELETE /api/person/{id}`

- `POST /api/category`
- `GET /api/category`

- `POST /api/transaction`
- `GET /api/transaction`

- `GET /api/totals/persons`
- `GET /api/totals/categories`

## Observações

- A documentação deste repositório foi centralizada neste arquivo para facilitar avaliação.
