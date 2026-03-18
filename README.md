# Controle de Gastos — Aplicação Full-Stack

Um aplicativo web completo para **gerenciamento de finanças pessoais** com foco em rastreamento de receitas, despesas, categorias e geração de relatórios consolidados.

## 📋 Visão Geral

**Controle de Gastos** permite que usuários:
- **Gerenciem pessoas** — registre múltiplos usuários com data de nascimento
- **Categorizem transações** — crie categorias de receita, despesa ou ambas
- **Registrem transações** — adicione receitas e despesas com descrição, valor, data e categoria
- **Visualizem relatórios** — consulte totalizações por pessoa e por categoria em tempo real

### Stack Tecnológico

| Componente | Tecnologia |
|-----------|-----------|
| **Frontend** | React 19 + TypeScript + Vite + Axios + React Router |
| **Backend** | .NET 8 + ASP.NET Core + Entity Framework Core + FluentValidation |
| **Banco de Dados** | PostgreSQL 15 |
| **Deploy** | Docker + Docker Compose (Oracle Cloud Always Free) |
| **Linting** | ESLint 9 + typescript-eslint |

## 📁 Arquitetura do Projeto

```
controle-gastos/
├── backend/                          # API .NET 8
│   ├── src/
│   │   ├── ControleGastos.API/      # Controllers, middleware, Program.cs
│   │   ├── ControleGastos.Application/  # Serviços, DTOs, validadores
│   │   ├── ControleGastos.Domain/   # Entidades, enums, interfaces
│   │   └── ControleGastos.Infrastructure/ # EF Core, migrações, repositórios
│   ├── tests/                        # Projetos de testes
│   ├── ControleGastos.sln           # Solution file
│   ├── Dockerfile                    # Multi-stage build para .NET
│   └── PROJECT_CONTEXT.md           # Documentação técnica detalhada
│
├── frontend/                         # SPA React + TypeScript
│   ├── controle-gastos-web/
│   │   ├── src/
│   │   │   ├── pages/               # Telas: People, Categories, Transactions, Reports
│   │   │   ├── services/            # Camada HTTP (Axios + apiClient)
│   │   │   ├── types/               # DTOs TypeScript
│   │   │   ├── auth/                # Contexto de autenticação
│   │   │   ├── utils/               # Utilitários (extractApiErrorMessage, etc)
│   │   │   └── App.tsx              # Rotas e navegação principal
│   │   ├── Dockerfile               # Multi-stage build (Node + Nginx)
│   │   ├── nginx.conf               # Configuração Nginx para SPA
│   │   ├── package.json             # Dependências
│   │   ├── vite.config.ts           # Configuração Vite
│   │   └── tsconfig.json            # Configuração TypeScript
│   └── PROJECT_CONTEXT.md           # Documentação técnica detalhada
│
├── docker-compose.yml               # Orquestração de containers
├── .env.example                     # Template de variáveis de ambiente
├── README.md                        # Este arquivo
└── PROJECT_CONTEXT.md              # Documentação geral

## 🚀 Quick Start — Desenvolvimento Local

### Pré-requisitos

- Node.js 18+ (frontend)
- .NET 8 SDK (backend)
- PostgreSQL 15+ (ou use Docker)
- Docker + Docker Compose (recomendado)

### Setup com Docker Compose (Recomendado)

1. **Clonar repositório:**
   ```bash
   git clone <url-repo>
   cd controle-gastos
   ```

2. **Criar arquivo `.env` (raiz do projeto):**
   ```env
   # Database
   DB_PASSWORD=postgres_dev_password
   CONNECTION_STRING=postgresql://postgres:postgres_dev_password@db:5432/controle_gastos?sslmode=disable

   # JWT
   JWT_SECRET=seu_jwt_secret_aqui_com_no_minimo_32_caracteres

   # Frontend
   VITE_API_BASE_URL=http://localhost:5034/api
   ```

3. **Iniciar containers:**
   ```bash
   docker-compose up -d
   ```

4. **Acessar aplicação:**
   - Frontend: `http://localhost:5173` (Vite dev server)
   - Backend API: `http://localhost:5034/api`
   - Swagger Docs: `http://localhost:5034/swagger`
   - PostgreSQL: `localhost:5432`

5. **Parar containers:**
   ```bash
   docker-compose down
   ```

### Setup Local (Sem Docker)

#### Backend

```bash
cd backend

# Restaurar dependências
dotnet restore

# Aplicar migrations (certifique-se de que PostgreSQL está rodando)
dotnet ef database update --project src/ControleGastos.Infrastructure --startup-project src/ControleGastos.API

# Executar testes
dotnet test

# Rodar em desenvolvimento
dotnet run --project src/ControleGastos.API
```

Backend estará disponível em `http://localhost:5034`.

#### Frontend

```bash
cd frontend/controle-gastos-web

# Instalar dependências
npm install

# Executar em desenvolvimento (com hot reload)
npm run dev

# Executar linting
npm run lint

# Build para produção
npm run build
```

Frontend estará disponível em `http://localhost:5173`.

## 🔌 API REST — Endpoints Principais

### Pessoas (`/api/person`)
- `GET /person` — listar todas as pessoas
- `GET /person/{id}` — obter pessoa específica  
- `POST /person` — criar nova pessoa
- `PUT /person/{id}` — atualizar pessoa
- `DELETE /person/{id}` — deletar pessoa (cascata: remove transações)

### Categorias (`/api/category`)
- `GET /category` — listar categorias
- `GET /category/{id}` — obter categoria
- `POST /category` — criar categoria

### Transações (`/api/transaction`)
- `GET /transaction` — listar transações
- `GET /transaction/{id}` — obter transação
- `POST /transaction` — criar transação

### Relatórios (`/api/totals`)
- `GET /totals/persons` — totalizações por pessoa
- `GET /totals/categories` — totalizações por categoria

## 📋 Regras de Negócio Implementadas

### Pessoas
- Nome obrigatório (máx. 200 caracteres)
- Data de nascimento obrigatória (não pode ser no futuro)
- Cálculo de idade automático em tempo real
- Exclusão em cascata: ao remover pessoa, todas suas transações também são removidas

### Categorias
- Descrição obrigatória (máx. 400 caracteres)
- Finalidade obrigatória: 
  - `Expense` (1) — apenas despesas
  - `Income` (2) — apenas receitas
  - `Both` (3) — receitas e despesas

### Transações
- Pessoa, categoria, descrição, valor e data obrigatórios
- Valor deve ser maior que zero
- Tipo obrigatório: `Expense` (1) ou `Income` (2)
- **Restrição:** menores de idade (< 18 anos) não podem registrar receitas
- Categorias filtradas por compatibilidade com tipo de transação
- Data persistida sem informação de timezone (compatível com PostgreSQL)

## 🛠️ Funcionalidades Frontend

- **Pessoas:** criar, listar, editar e deletar usuários
- **Categorias:** criar e listar categorias (edição aguardando)
- **Transações:** criar e listar transações (edição/exclusão aguardando)
- **Relatórios:** visualizar totalizações consolidadas por pessoa e categoria
- **Validação:** regras de negócio reforçadas no cliente (UX melhorada)
- **Tratamento de Erros:** normalização de mensagens de erro em `extractApiErrorMessage`
- **Responsividade:** layout adaptativo para mobile/tablet

## 🔐 Autenticação e Segurança

**Status:** ✅ **Implementado** — JWT + OAuth Google + Refresh Tokens com cookies HttpOnly

### Recursos Implementados

#### Login/Registro (Frontend)
- Email/senha com validação
- OAuth 2.0 com Google (Sign in with Google button)
- Link automático de conta Google com email existente
- Formulário em `/auth` (página pública)

#### Backend Auth Flow
- `POST /api/auth/register` — criar conta novo
- `POST /api/auth/login` — login email+senha
- `POST /api/auth/google` — login via Google OAuth
- `POST /api/auth/refresh` — renovar token expirado
- `POST /api/auth/logout` — logout + revoga refresh token
- `GET /api/users/me` — retorna usuário autenticado

#### Segurança
- ✅ **JWT** (HS256) armazenado em memória (React state)
- ✅ **RefreshToken** em **cookie HttpOnly** (previne XSS)
- ✅ **Password** com PBKDF2 hash (nunca texto plano)
- ✅ **Token Refresh** automático (interceptador Axios)
- ✅ **CORS** com credenciais apenas de `http://localhost:5173`
- ✅ **Revogação** de tokens possível (RevokedAt na DB)
- ✅ **Email normalizado** (trim, lowercase)
- ✅ **SameSite=Lax** nas cookies (anti-CSRF)

#### Rotas Protegidas
- `/reports` — Apenas autenticados
- `/transactions` — Apenas autenticados
- `/people` — Apenas autenticados
- `/categories` — Apenas autenticados
- `/auth` — Apenas não autenticados (redireciona se logado)

**Token Expiration:** Access token 15min, Refresh token 7 dias

### Variáveis de Ambiente
```env
Auth__Jwt__Secret=seu_jwt_secret_minimo_32_caracteres
Auth__Jwt__AccessTokenMinutes=15
Auth__Jwt__RefreshTokenDays=7
Auth__Google__ClientId=seu_google_client_id.apps.googleusercontent.com
```

## 📱 UX e Design

- Layout centralizado (`max-width: 980px`) com cartões brancos
- Navegação superior em abas com estado ativo destacado
- Formulários lineares com feedback de erro/sucesso
- Tabelas com scroll horizontal em telas pequenas
- Breakpoints responsivos para mobile/tablet
- Formatação monetária em pt-BR com moeda BRL

## 🧪 Testes

### Backend
```bash
cd backend

# Rodar todos os testes
dotnet test

# Rodar com cobertura
dotnet test /p:CollectCoverage=true

# Rodar teste específico
dotnet test --filter "Category~TestName"
```

**Cobertura Atual:** ~43 testes
- Domain: validações de entidade
- Application: regras de negócio
- API Integration: fluxos end-to-end
- Validators: FluentValidation

### Frontend
```bash
cd frontend/controle-gastos-web

# ESLint
npm run lint

# Build (verifica erros TypeScript)
npm run build

# Preview da build
npm run preview
```

## 🐳 Deploy em Produção

### Oracle Cloud Always Free

Consulte o guia completo em [DEPLOY_ORACLE_CLOUD.md](./DEPLOY_ORACLE_CLOUD.md) (em planejamento) para:

- Provisionar VM Ubuntu (1 OCPU, 1GB RAM) e Autonomous Database PostgreSQL (20GB) — **gratuito**
- Configurar Docker, Nginx e HTTPS com Let's Encrypt
- Executar migrations em produção
- Monitorar aplicação e banco de dados

**Resumo Rápido:**
```bash
# Na VM Oracle Cloud
git clone <repo>
cd controle-gastos

# Configurar .env
VITE_API_BASE_URL=https://seu_dominio.com/api
CONNECTION_STRING=postgresql://postgres:senha@host:5432/controle_gastos

# Build e deploy
docker-compose build
docker-compose up -d

# Executar migrations
docker-compose exec api dotnet ef database update --project ControleGastos.Infrastructure
```

### Variáveis de Ambiente em Produção

```env
# API
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=postgresql://postgres:password@host:5432/controle_gastos

# JWT
Auth__Jwt__Secret=seu_jwt_secret_minimo_32_caracteres
Auth__Jwt__AccessTokenMinutes=60
Auth__Jwt__RefreshTokenDays=30

# Frontend
VITE_API_BASE_URL=https://seu_dominio.com/api

# Database
DB_PASSWORD=sua_senha_segura
```

## 📚 Documentação Técnica Detalhada

- **[backend/PROJECT_CONTEXT.md](./backend/PROJECT_CONTEXT.md)** — Domain model, API contract, configuração, migrations
- **[frontend/PROJECT_CONTEXT.md](./frontend/PROJECT_CONTEXT.md)** — Stack, rotas, serviços HTTP, tipagem completa

## 📊 Estrutura de Dados (Domain Model)

### Person
```csharp
{
  id: UUID,
  name: string (1-200 chars),
  birthDate: date,
  age: int (calculado em runtime)
}
```

### Category
```csharp
{
  id: UUID,
  description: string (1-400 chars),
  purpose: int (1=Expense, 2=Income, 3=Both)
}
```

### Transaction
```csharp
{
  id: UUID,
  personId: UUID,
  categoryId: UUID,
  description: string (1-400 chars),
  value: decimal(18,2),
  date: date,
  type: int (1=Expense, 2=Income)
}
```

### Report Summary
```csharp
{
  totalIncome: decimal,
  totalExpense: decimal,
  balance: decimal
}
```

## ✅ Checklist: Pronto para Produção?

- [x] Autenticação JWT + Google OAuth implementada
- [x] Rotas protegidas com refresh token automático
- [ ] HTTPS configurado (Let's Encrypt em Oracle Cloud)
- [ ] Migrations de banco executadas
- [ ] Variáveis de ambiente configuradas em produção
- [ ] CORS ajustado para domínio de produção
- [ ] Google OAuth Client ID obtido do console Google
- [ ] Testes de carga e segurança
- [ ] Backup automático configurado (Autonomous DB)
- [ ] Monitoramento e logs centralizados
- [ ] CI/CD pipeline (GitHub Actions/GitLab)
- [ ] Swagger protegido ou desabilitado em produção

## 🚦 Limitações e Gaps Conhecidos

- ❌ Edição/exclusão de categorias (apenas CRUD parcial, não há PUT/DELETE)
- ❌ Edição/exclusão de transações (apenas leitura)
- ❌ Paginação nas listagens
- ❌ Filtros avançados (busca, período, etc)
- ❌ Cache compartilhado entre telas (estado local por página)
- ❌ Testes E2E automatizados
- ❌ Internacionalização i18n (hardcoded pt-BR)
- ⚠️ Swagger desabilitado em produção (considerar habilitar com restrições)

## 🚀 Próximas Prioridades

1. ✅ **Autenticação JWT** — ~~Implementar login/logout e proteção de rotas~~ (CONCLUÍDO)
2. **Deploy Oracle Cloud** — Provisionar infraestrutura e publicar live
3. **Testes E2E** — Adicionar testes com Cypress/Playwright
4. **Edição de Transações** — Permitir editar/deletar transações existentes
5. **Paginação** — Suporte a pagination nas listagens
6. **Filtros Avançados** — Busca, período, categoria e tipo
7. **Observabilidade** — Logs estruturados, métricas e rastreamento

## 🤝 Contribuindo

1. Faça uma branch: `git checkout -b feature/nova-feature`
2. Commit suas mudanças: `git commit -am 'Descrição clara da mudança'`
3. Push para a branch: `git push origin feature/nova-feature`
4. Abra um Pull Request
5. Certifique-se de:
   - Passar em ESLint/TypeScript (frontend): `npm run lint`
   - Passar em testes (backend): `dotnet test`
   - Build sem erros: `npm run build` (frontend) e `dotnet build` (backend)

## 🐛 Debugging e Logs

### Backend
```bash
# Logs detalhados durante desenvolvimento
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/ControleGastos.API

# Logs em arquivo real-time (Docker)
docker-compose logs -f api

# Logs do banco de dados
docker-compose logs -f db
```

### Frontend
```bash
# Console do navegador (F12) mostra requisições HTTP
# Logs customizados em src/services/apiClient.ts:
const DEBUG = true;  // Ativa logs de requisições
```

## 📞 Suporte

- **Issues:** Abra uma issue no repositório
- **Documentação API:** Swagger em `/swagger` (apenas em desenvolvimento)
- **Contato:** [seu-email@example.com]

## 📄 Licença

MIT License

---

**Última atualização:** 17 de março de 2026

**Stack resumido:**
- **Backend:** .NET 8 + ASP.NET Core + EF Core + PostgreSQL
- **Frontend:** React 19 + TypeScript + Vite + Axios
- **Deploy:** Docker Compose + Oracle Cloud Always Free (planejado)
