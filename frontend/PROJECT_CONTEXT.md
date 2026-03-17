# PROJECT_CONTEXT.md — Frontend (`controle-gastos-web`)

## 1) Visão geral

Este frontend implementa uma interface web para **controle de gastos**, com foco em:
- Cadastro e gestão de pessoas.
- Cadastro de categorias financeiras.
- Registro de transações (receitas e despesas).
- Visualização de relatórios consolidados por pessoa e por categoria.

O app é SPA em React + TypeScript e consome uma API HTTP (backend separado).

## 2) Stack e versões principais

- **Runtime/bundler:** Vite `^7.3.1`
- **UI:** React `^19.2.0`, React DOM `^19.2.0`
- **Roteamento:** React Router DOM `^7.13.0`
- **HTTP client:** Axios `^1.13.5`
- **Linguagem:** TypeScript `~5.9.3`
- **Lint:** ESLint 9 + `typescript-eslint` + `react-hooks` + `react-refresh`

Arquivo de referência: [`controle-gastos-web/package.json`](./controle-gastos-web/package.json)

## 3) Estrutura do projeto (frontend)

Raiz atual (`frontend/`) contém:
- `controle-gastos-web/` → aplicação Vite/React.
- `PROJECT_CONTEXT.md` (este documento).

Estrutura relevante de `controle-gastos-web/src`:
- `main.tsx` → bootstrap React + BrowserRouter.
- `App.tsx` → navegação principal e rotas.
- `pages/` → telas de domínio:
  - `people/PeoplePage.tsx`
  - `categories/CategoriesPage.tsx`
  - `transactions/TransactionsPage.tsx`
  - `reports/ReportsPage.tsx`
- `services/` → camada de acesso HTTP:
  - `apiClient.ts`
  - `personService.ts`
  - `categoryService.ts`
  - `transactionService.ts`
  - `reportService.ts`
- `types/` → contratos tipados para domínio/API:
  - `person.ts`, `category.ts`, `transaction.ts`, `report.ts`
- `utils/extractApiErrorMessage.ts` → normalização de mensagens de erro da API.

## 4) Inicialização e roteamento

### Bootstrap
Em `main.tsx`:
- Renderiza `App` dentro de `StrictMode`.
- Usa `BrowserRouter` para rotas client-side.

### Rotas
Definidas em `App.tsx`:
- `/` redireciona para `/reports`
- `/reports` → `ReportsPage`
- `/transactions` → `TransactionsPage`
- `/people` → `PeoplePage`
- `/categories` → `CategoriesPage`

A navegação superior usa `NavLink` com estado ativo.

## 5) Arquitetura e padrão de código

Padrão predominante:
- **Página (UI + estado local + regras de formulário) + Service (HTTP) + Types (contratos)**.
- Cada página controla seu próprio ciclo de carregamento (`isLoading`), envio (`isSaving`), erro e sucesso.
- Erros de API são convertidos para texto amigável via `extractApiErrorMessage`.
- Sem state manager global (Redux/Zustand/etc.); estado é local por tela.
- Estilização por CSS separado por página (escopo por classe, sem CSS-in-JS).

## 6) Configuração de API e ambiente

Em `src/services/apiClient.ts`:
- Base URL padrão: `http://localhost:5034/api`
- Pode ser sobrescrita por variável de ambiente:
  - `VITE_API_BASE_URL`
- Header padrão: `Content-Type: application/json`

Implicação prática:
- Para ambientes diferentes (dev/homolog/prod), usar `.env` com `VITE_API_BASE_URL`.

## 7) Contratos de domínio (TypeScript)

## Pessoas
- `Person`: `{ id, name, birthDate, age }`
- `PersonDto`: `{ name, birthDate }`

## Categorias
- Enum-like `CategoryPurpose`:
  - `Expense = 1`
  - `Income = 2`
  - `Both = 3`
- `Category`: `{ id, description, purpose }`
- `CategoryDto`: `{ description, purpose }`

## Transações
- Enum-like `TransactionType`:
  - `Expense = 1`
  - `Income = 2`
- `Transaction`: `{ id, description, value, date, personId, categoryId, type }`
- `CreateTransactionDto`: `{ personId, description, value, categoryId, transactionType, date }`

## Relatórios
- `TotalsSummary`: `{ totalIncome, totalExpense, balance }`
- `PersonTotalsResponse`: `{ items: PersonTotalsItem[], summary }`
- `CategoryTotalsResponse`: `{ items: CategoryTotalsItem[], summary }`

## 8) Endpoints consumidos

### Pessoas (`personService.ts`)
- `GET /person` → listar pessoas.
- `POST /person` → criar pessoa.
- `PUT /person/{id}` → atualizar pessoa.
- `DELETE /person/{id}` → excluir pessoa.

### Categorias (`categoryService.ts`)
- `GET /category` → listar categorias.
- `POST /category` → criar categoria.

### Transações (`transactionService.ts`)
- `GET /transaction` → listar transações.
- `POST /transaction` → criar transação.

### Relatórios (`reportService.ts`)
- `GET /totals/persons` → totais por pessoa.
- `GET /totals/categories` → totais por categoria.

## 9) Regras de negócio implementadas no frontend

## PeoplePage
- Nome obrigatório.
- Nome com máximo de 200 caracteres.
- Data de nascimento obrigatória.
- Data de nascimento deve ser anterior ao dia atual.
- Mesmo formulário atende criação e edição.
- Ao excluir pessoa, UI avisa que as transações associadas também serão removidas (dependência de regra no backend).

## CategoriesPage
- Descrição obrigatória.
- Descrição com máximo de 400 caracteres.
- Finalidade obrigatória (`Expense`, `Income`, `Both`).

## TransactionsPage
- Pessoa obrigatória.
- Descrição obrigatória (máx. 400).
- Valor obrigatório e > 0.
- Tipo obrigatório (`Expense`/`Income`).
- Categoria obrigatória.
- Data obrigatória.
- Se pessoa selecionada for menor de idade (`age < 18`), transação de **receita** é bloqueada na UI e validada novamente antes de enviar.
- Categorias são filtradas por compatibilidade com o tipo da transação:
  - `Both` aceita ambos.
  - `Expense` aceita apenas despesa.
  - `Income` aceita apenas receita.
- Data enviada no payload como `YYYY-MM-DDT00:00:00` (sem timezone explícito), para compatibilidade com `timestamp without time zone` no backend (comentário no código).

## ReportsPage
- Busca paralela de dados por pessoa e categoria (`Promise.all`).
- Exibe tabelas e cards-resumo (receitas, despesas, saldo).
- Formatação monetária em `pt-BR` com moeda `BRL`.

## 10) Tratamento de erros

Função central: `extractApiErrorMessage(error)`
- Se não for erro Axios: retorna mensagem genérica.
- Ordem de prioridade para mensagem da resposta:
  1. `response.data.message`
  2. `response.data.title`
  3. Primeiro item de `response.data.errors` (objeto de validação)
- Fallback final: mensagem genérica de falha da solicitação.

Isso reduz duplicação e padroniza feedback ao usuário em todas as páginas.

## 11) Estilo visual e UX atual

- Layout centralizado (`max-width: 980px`) com cartões brancos.
- Navegação superior em “pílulas” com link ativo destacado.
- Formulários simples, lineares e com feedback textual de erro/sucesso.
- Tabelas com `overflow-x: auto` para telas menores.
- Breakpoints simples para responsividade (ex.: ações empilhadas no mobile em `PeoplePage`; cards de resumo em coluna no `ReportsPage`).

## 12) Scripts e fluxo de desenvolvimento

Em `package.json`:
- `npm run dev` → sobe Vite em modo desenvolvimento.
- `npm run build` → `tsc -b` + build de produção Vite.
- `npm run lint` → ESLint no projeto.
- `npm run preview` → preview da build.

## 13) Configuração TypeScript

- Projeto usa **TS project references**:
  - `tsconfig.app.json` (código frontend em `src`)
  - `tsconfig.node.json` (config de tooling, ex.: `vite.config.ts`)
- Principais opções:
  - `strict: true`
  - `noUnusedLocals`, `noUnusedParameters`
  - `moduleResolution: bundler`
  - `jsx: react-jsx`
  - `noEmit: true` (build emitido via Vite)

## 14) Dependências e acoplamentos com backend

Dependências implícitas de contrato:
- Backend deve expor endpoints e formatos descritos acima.
- `CategoryPurpose` e `TransactionType` usam valores numéricos específicos (1/2/3 e 1/2).
- Backend envia `age` em `Person` (cálculo de menoridade depende disso no frontend).
- Backend precisa aceitar data de transação no formato `YYYY-MM-DDT00:00:00`.

## 15) Limitações/gaps atuais (importante para IA)

- Não há autenticação/autorização no frontend.
- Não há testes automatizados (unitário/integrado/E2E) no código atual.
- Não há paginação/filtros avançados nas listagens.
- Não há edição/exclusão para categorias e transações (apenas criação/listagem).
- Camada de estado é local por página; sem cache compartilhado entre telas.
- Não há normalização internacional completa (texto está em pt-BR fixo).

## 16) Diretrizes para futuras alterações (contexto para IA)

Ao pedir mudanças para IA neste projeto, informar preferencialmente:
- Qual página/módulo será alterado (`pages/*`, `services/*`, `types/*`).
- Se haverá alteração de contrato de API (payload/response/endpoints).
- Se a regra de negócio é frontend-only ou precisa ser validada também no backend.
- Como tratar mensagens de erro (manter `extractApiErrorMessage`).
- Se novos enum-like numéricos devem manter compatibilidade com backend.
- Critérios de UX esperados (feedback de loading/sucesso/erro e responsividade).

## 17) Comandos rápidos úteis

No diretório `controle-gastos-web`:

```bash
npm install
npm run dev
npm run lint
npm run build
npm run preview
```

Configuração opcional de ambiente (`.env`):

```env
VITE_API_BASE_URL=http://localhost:5034/api
```

## 18) Deploy e Produção

### Build para Produção

```bash
# Verificar tipos TypeScript
tsc -b

# Build Vite (output em dist/)
npm run build

# Preview da build
npm run preview
```

### Configuração para Produção

1. **Variáveis de Ambiente:**
   - Criar `.env.production` com `VITE_API_BASE_URL` apontando para domínio de produção.
   - Exemplo: `VITE_API_BASE_URL=https://app.seu-dominio.com/api`

2. **Dockerfile para Produção:**
   - Multi-stage: build Node → output dist → serve via Nginx.
   - Otimizado para Alpine (menor imagem).
   - Volume de dist servido como read-only.

3. **Nginx Configuration:**
   - SPA routing (try_files $uri $uri/ /index.html)
   - Reverse proxy para API: `/api/` → `http://api:80`
   - Headers de segurança (CORS ajustado para domínio)
   - Compressão gzip habilitada

4. **CORS em Produção:**
   - Backend deve aceitar apenas origem: `https://seu_dominio.com`
   - Remove credenciais se não necessário.

### Docker Compose (Integração)

O serviço `web` usa:
```yaml
web:
  build:
    context: ./frontend/controle-gastos-web
    dockerfile: Dockerfile
  depends_on:
    - api
  networks:
    - app_network
  restart: unless-stopped
```

## 19) Monitoramento e Logs

### Frontend com Erros

- Logs de requisições HTTP em `src/services/apiClient.ts` (when DEBUG=true).
- Console do navegador (F12) mostra:
  - Requisições/respostas Axios.
  - Erros de renderização React.
  - State dumps.

### Tratamento de Erros em Produção

- Função `extractApiErrorMessage` normaliza erros de API.
- Mensagens de erro exibidas em modais/toasts.
- Logs de erro para análise posterior (telemetria futura).

## 20) Performance e Otimizações

### Vite
- Code splitting automático.
- Tree-shaking de dependências não utilizadas.
- Source maps em dev, minificado em produção.
- `moduleResolution: bundler` no tsconfig.

### Lazy Loading de Rotas

Recomendação futura:
```tsx
const PeoplePage = lazy(() => import('./pages/people/PeoplePage'));
const CategoriesPage = lazy(() => import('./pages/categories/CategoriesPage'));
```

## 21) Segurança

- **HTTPS obrigatório em produção** (Let's Encrypt via Certbot).
- **JWT Bearer Token:** será implementado em cookies HTTP-only após autenticação.
- **CSRF tokens:** adicionar quando autenticação for implementada.
- **Content Security Policy (CSP):** considerar adicionar headers no Nginx.
- **XSS Prevention:** React sanitiza conteúdo por padrão.

## 22) Resumo executivo

Frontend React/TypeScript organizado por domínio (páginas + services + types), com regras de validação no cliente para pessoas/categorias/transações e módulo de relatórios consolidados. Integração com backend REST é direta via Axios e depende de enums numéricos e contratos tipados explícitos. O projeto está funcional para CRUD parcial e consulta de relatórios, com espaço claro para evolução em testes, autenticação e operações avançadas.

**Para deploy em Oracle Cloud Always Free:**
- Usar Dockerfile multi-stage com Node 18 Alpine + Nginx Alpine.
- Configurar nginx.conf com SPA routing e reverse proxy para API.
- Variáveis de ambiente em .env com VITE_API_BASE_URL apontando para domínio/IP.
- HTTPS via Let's Encrypt com renovação automática via Certbot.
