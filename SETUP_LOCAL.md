# Setup Local — Guia Passo-a-Passo

Este documento fornece instruções detalhadas para configurar o ambiente de desenvolvimento local para o projeto **Controle de Gastos**.

## 🎯 Objetivo

Configurar a aplicação full-stack (backend .NET 8 + frontend React) em máquina local com:
- Backend rodando em `http://localhost:5034`
- Frontend rodando em `http://localhost:5173`
- PostgreSQL em `localhost:5432`

**Tempo estimado:** 20-30 minutos (assuming Docker is installed)

---

## 📋 Pré-Requisitos

### Opção 1: Com Docker (Recomendado)
- Docker Desktop instalado ([download aqui](https://www.docker.com/products/docker-desktop))
- Docker Compose (`docker-compose --version` deve retornar versão)
- Git instalado

### Opção 2: Sem Docker (Manual)
- .NET 8 SDK ([download aqui](https://dotnet.microsoft.com/download/dotnet/8.0))
- Node.js 18+ ([download aqui](https://nodejs.org/))
- PostgreSQL 15+ ([download aqui](https://www.postgresql.org/download/))
- Git instalado

---

## 🚀 Setup com Docker Compose (Recomendado)

### Passo 1: Clonar Repositório

```bash
git clone <seu-repo-url>
cd controle-gastos
```

### Passo 2: Criar Arquivo `.env`

Na **raiz do projeto** (mesma pasta que `docker-compose.yml`), crie um arquivo `.env`:

```env
# ===== DATABASE =====
DB_PASSWORD=postgres_dev_password
CONNECTION_STRING=postgresql://postgres:postgres_dev_password@db:5432/controle_gastos?sslmode=disable

# ===== JWT (Autenticação — futuro) =====
JWT_SECRET=sua_chave_secreta_aqui_com_no_minimo_32_caracteres

# ===== FRONTEND =====
VITE_API_BASE_URL=http://localhost:5034/api
```

**Notas importantes:**
- `DB_PASSWORD`: pode ser qualquer coisa em dev (exemplo: `postgres_dev_password`)
- `JWT_SECRET`: deve ter mínimo 32 caracteres
- `CONNECTION_STRING`: formato PostgreSQL com sslmode=disable (seguro em dev)

### Passo 3: Iniciar Containers

```bash
# Build e inicie os containers
docker-compose up -d

# Verifique se está tudo rodando
docker-compose ps
```

Você deve ver 3 containers:
- `controle-gastos-db-1` (PostgreSQL) — status: `running`
- `controle-gastos-api-1` (Backend .NET) — status: `running`
- `controle-gastos-web-1` (Frontend React + Nginx) — status: `running`

### Passo 4: Acessar Aplicação

Abra no navegador:

| Componente | URL | Descrição |
|-----------|-----|-----------|
| **Frontend** | `http://localhost:5173` | Aplicação React (Vite dev server) |
| **Backend API** | `http://localhost:5034/api` | API REST base URL |
| **Swagger Docs** | `http://localhost:5034/swagger` | Documentação interativa da API |
| **PostgreSQL** | `localhost:5432` | Banco de dados (host:port) |

### Passo 5: Parar Containers (quando terminar)

```bash
# Parar containers (mantém dados do banco)
docker-compose down

# Remover tudo (inclui volume do banco)
docker-compose down -v
```

---

## 🛠️ Setup Local (Sem Docker)

Se você não quer/pode usar Docker, siga estes passos para configurar tudo localmente.

### Backend Setup

#### Passo 1: Instalar PostgreSQL

1. Download e instale [PostgreSQL 15+](https://www.postgresql.org/download/)
2. Durante instalação, anote a senha do usuário `postgres`
3. Verifique da instalação:
   ```bash
   psql --version
   ```

#### Passo 2: Criar Banco de Dados

```bash
# Conectar ao PostgreSQL
psql -U postgres

# No prompt psql, execute:
CREATE DATABASE controle_gastos;
\q
```

#### Passo 3: Clonar Repositório

```bash
git clone <seu-repo-url>
cd controle-gastos/backend
```

#### Passo 4: Configurar Connection String

Edite `src/ControleGastos.API/appsettings.json`:

```json
{
  "Logging": { ... },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=controle_gastos;Username=postgres;Password=sua_senha_postgres"
  },
  "AllowedHosts": "*"
}
```

Substitua `sua_senha_postgres` pela senha que você definiu durante instalação do PostgreSQL.

#### Passo 5: Restaurar Dependências

```bash
cd backend
dotnet restore
```

#### Passo 6: Aplicar Migrations

```bash
dotnet ef database update \
  --project src/ControleGastos.Infrastructure \
  --startup-project src/ControleGastos.API
```

Você deve ver output tipo:
```
...done.
Applying migration '20XX0101000000_InitialCreate'.
Done. Your database is ready.
... (mais migrations)
Applying migration '20XX0101000001_AddRefreshTokens'.
Done. Your database is ready.
```

#### Passo 7: Executar Backend

```bash
dotnet run --project src/ControleGastos.API
```

Output esperado:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5034
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit.
```

Backend agora está em `http://localhost:5034`.

### Frontend Setup

#### Passo 1: Instalar Node.js

1. Download [Node.js 18+](https://nodejs.org/) (recomendado LTS)
2. Verifique instalação:
   ```bash
   node --version
   npm --version
   ```

#### Passo 2: Instalar Dependências

```bash
cd frontend/controle-gastos-web
npm install
```

Isso instala todas as dependências listadas em `package.json`.

#### Passo 3: Configurar URL da API (opcional)

Se seu backend não está em `http://localhost:5034/api`, crie um arquivo `.env`:

```env
VITE_API_BASE_URL=http://localhost:5034/api
```

**Padrão:** Se não criar `.env`, o frontend usa `http://localhost:5034/api`.

#### Passo 4: Executar Frontend

```bash
npm run dev
```

Output esperado:
```
  VITE v7.x.x  ready in 234 ms

  ➜  Local:   http://localhost:5173/
  ➜  press h to show help
```

Frontend agora está em `http://localhost:5173`.

#### Passo 5: Abrir no Navegador

Abra `http://localhost:5173` e você deve ver a aplicação carregada.

---

## ✅ Testes de Conexão

### Testar Backend Manualmente

```bash
# Verificar se API está respondendo
curl http://localhost:5034/api/person

# Você deve receber um JSON (sucesso ou erro esperado)
[]  # ou { "type": "...", "message": "..." }
```

### Testar Banco de Dados

```bash
# Conectar ao banco
psql -U postgres -d controle_gastos

# Ver tabelas criadas
\dt

# Ver dados (exemplo)
SELECT * FROM "People";
\q
```

---

## 🧪 Executar Testes

### Backend (Unit + Integration Tests)

```bash
cd backend

# Rodar todos os testes
dotnet test

# Rodar com mais verbosidade
dotnet test --verbosity normal

# Rodar teste específico
dotnet test --filter "ClassNameTest"
```

Esperado: todos os testes passam (43+ testes).

### Frontend (Linting)

```bash
cd frontend/controle-gastos-web

# ESLint
npm run lint

# Build (verifica TypeScript)
npm run build
```

---

## 🚀 Scripts Úteis

### Backend

```bash
cd backend

# Restaurar + Build + Testes + Run
dotnet restore
dotnet build
dotnet test
dotnet run --project src/ControleGastos.API

# Criar nova migration
dotnet ef migrations add NomeDaMigracao --project src/ControleGastos.Infrastructure

# Ver migrations pendentes
dotnet ef migrations list --project src/ControleGastos.Infrastructure
```

### Frontend

```bash
cd frontend/controle-gastos-web

# Dev server com hot reload
npm run dev

# Build para produção
npm run build

# Preview da build
npm run preview

# Lint
npm run lint

# Lint com fix automático
npm run lint --fix
```

---

## 🐛 Troubleshooting

### Erro: "Cannot connect to database"

**Causa:** PostgreSQL não está rodando ou connection string está errada.

**Solução:**
1. Verif que PostgreSQL está rodando: `psql -U postgres`
2. Verificar connection string em `appsettings.json`
3. Verificar senha do usuário `postgres`

### Erro: "Port 5173 is already in use"

**Causa:** Outro processo está usando porta 5173.

**Solução:**
```bash
# Encontrar processo na porta
lsof -i :5173  # Mac/Linux
netstat -ano | findstr :5173  # Windows

# Matar processo
kill -9 <PID>  # Mac/Linux
taskkill /PID <PID> /F  # Windows

# Ou especificar porta diferente
npm run dev -- --port 3000
```

### Erro: "CORS error: No 'Access-Control-Allow-Origin' header"

**Causa:** Backend não está aceitando requisições do frontend.

**Solução:**
1. Verificar que backend está rodando em `http://localhost:5034`
2. Verificar CORS em `src/ControleGastos.API/Program.cs`
3. Confirmar que frontend está em `http://localhost:5173`

### Erro: "npm: command not found"

**Causa:** Node.js não está instalado ou PATH não está configurado.

**Solução:**
1. Instalar Node.js 18+: https://nodejs.org/
2. Restart terminal
3. Verificar: `npm --version`

### Docker container exita immediately

**Causa:** Erro na inicialização do container.

**Solução:**
```bash
# Ver logs
docker-compose logs api

# Rebuild
docker-compose build --no-cache

# Restart
docker-compose up -d
```

---

## 📞 Próximos Passos

1. ✅ Setup local concluído
2. 🧪 Explorar funcionalidades:
   - Criar pessoa (http://localhost:5173/people)
   - Criar categoria (http://localhost:5173/categories)
   - Criar transação (http://localhost:5173/transactions)
   - Ver relatórios (http://localhost:5173/reports)
3. 📖 Ler documentação técnica:
   - [backend/PROJECT_CONTEXT.md](./backend/PROJECT_CONTEXT.md)
   - [frontend/PROJECT_CONTEXT.md](./frontend/PROJECT_CONTEXT.md)
4. 🚀 Próximo: Deploy em Oracle Cloud Always Free (veja [DEPLOY_ORACLE_CLOUD.md](./DEPLOY_ORACLE_CLOUD.md))

---

## 📚 Referências

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [React 19 Documentation](https://react.dev)
- [TypeScript Documentation](https://www.typescriptlang.org/docs/)
- [Vite Documentation](https://vitejs.dev/)
- [Docker Documentation](https://docs.docker.com/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

**Última atualização:** 17 de março de 2026

**Problemas?** Abra uma issue no repositório com os logs e descrição do erro.
