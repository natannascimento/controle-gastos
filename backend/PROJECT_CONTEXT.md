# PROJECT CONTEXT - Controle de Gastos Backend

## 1. Overview

This repository contains a backend API for personal expense control built with `.NET 8`, `ASP.NET Core`, `Entity Framework Core`, and `PostgreSQL`.

The project is structured by layers:

- `ControleGastos.API`: HTTP layer, DI setup, middleware, controllers.
- `ControleGastos.Application`: use cases/services, DTOs, validators, application exceptions.
- `ControleGastos.Domain`: entities, enums, repository contracts, report models.
- `ControleGastos.Infrastructure`: EF Core context, migrations, repository implementations.
- `tests/*`: unit and integration test projects.

## 2. Domain Model

### User
- Fields: `Id`, `Email`, `PasswordHash`, `GoogleSub`, `AuthProvider`, `PersonId`
- Derived field: `Person` (relacionamento 1:1)
- AuthProvider enum: `Email = 1`, `Google = 2`
- Rules in entity:
  - Email obrigatório e único (normalized)
  - PasswordHash requerido (nunca null, até para Google)
  - GoogleSub único (se Google)
  - PersonId requerido (criada automaticamente no login)

### RefreshToken
- Fields: `Id`, `UserId`, `TokenHash`, `ExpiresAt`, `RevokedAt`
- Rules:
  - TokenHash é SHA256 (nunca raw token armazenado)
  - ExpiresAt: DateTime em UTC
  - RevokedAt: null=ativo, DateTime=revogado
  - Method: `IsActive(nowUtc)` verifica ativo e não expirado

### Person

- Fields: `Id`, `Name`, `BirthDate`.
- Derived field: `Age` (calculated dynamically from `BirthDate`).
- Rules in entity:
  - Name cannot be empty.
  - BirthDate cannot be in the future.

### Category

- Fields: `Id`, `Description`, `Purpose`.
- `Purpose` enum:
  - `Expense = 1`
  - `Income = 2`
  - `Both = 3`
- Rules in entity:
  - Description required, max 400 chars.
  - Purpose must be a valid enum value.

### Transaction

- Fields: `Id`, `PersonId`, `CategoryId`, `Description`, `Value`, `Date`, `Type`.
- `Type` enum:
  - `Expense = 1`
  - `Income = 2`
- Rules in entity:
  - PersonId and CategoryId must not be empty GUID.
  - Description required, max 400 chars.
  - Value must be greater than zero.
  - Type must be valid enum.
  - Date must be non-default.

## 3. Business Rules (Application Layer)

Main rules are enforced in `TransactionService`:

1. Transaction value must be positive.
2. Person must exist.
3. Category must exist.
4. Minors (`Age < 18`) cannot register `Income`.
5. Category purpose must match transaction type (`Both` accepts both).

Error messages are centralized in `BusinessErrorMessages`.

## 4. API Surface

### Authentication (`/api/auth`)
- `POST /auth/register` — registrar novo usuário
  - Body: `{ email, password, name, birthDate }`
  - Response: `{ accessToken, expiresIn, user }`
- `POST /auth/login` — login com email/senha
  - Body: `{ email, password }`
  - Response: `{ accessToken, expiresIn, user }`
  - Cookie: `cg_refresh` (HttpOnly, Secure, SameSite=Lax)
- `POST /auth/google` — login com Google OAuth
  - Body: `{ idToken }`
  - Response: `{ accessToken, expiresIn, user }`
  - Cookie: `cg_refresh` (HttpOnly)
- `POST /auth/refresh` — renovar token
  - Body: empty (usa cookie)
  - Response: `{ accessToken, expiresIn, user }`
  - Cookie: novo `cg_refresh`
- `POST /auth/logout` — logout e revoga refresh
  - [Authorize] — requer JWT
  - Response: 204 No Content
  - Cookie: `cg_refresh` deletado

### Users (`/api/users`)
- `GET /users/me` — retorna usuário autenticado
  - [Authorize] — requer JWT
  - Response: `{ id, email, name, authProvider, personId }`

### Person

- `POST /api/person`
- `GET /api/person/{id}`
- `GET /api/person`
- `PUT /api/person/{id}`
- `DELETE /api/person/{id}`

### Category

- `POST /api/category`
- `GET /api/category/{id}`
- `GET /api/category`

### Transaction

- `POST /api/transaction`
- `GET /api/transaction/{id}`
- `GET /api/transaction`

### Totals

- `GET /api/totals/persons`
- `GET /api/totals/categories`

## 5. Validation

Validation uses `FluentValidation` for input DTOs:

- `RegisterValidator` — registrar novo usuário
  - Email: não vazio, formato email, máx 200 chars
  - Password: mín 8 chars, pelo menos 1 maiúscula, 1 número, 1 símbolo
  - Name: não vazio, máx 200 chars
  - BirthDate: data válida, não no futuro
- `LoginValidator` — login
  - Email: não vazio, formato email
  - Password: não vazio
- `GoogleLoginValidator` — Google OAuth
  - IdToken: não vazio
- `PersonValidator`
- `CategoryValidator`
- `TransactionValidator`

`ApiBehaviorOptions.InvalidModelStateResponseFactory` was customized to return the project error envelope format for model/validation failures.

## 6. Error Contract

The API uses a custom error envelope:

```json
{
  "type": "validation_error|business_rule|not_found|unexpected_error",
  "message": "human readable message",
  "errors": {
    "FieldName": ["error 1", "error 2"]
  }
}
```

Handled globally by `ExceptionHandlingMiddleware`:

- `ValidationException` -> `400 validation_error`
- `BusinessRuleException` -> `400 business_rule`
- `NotFoundException` -> `404 not_found`
- `Exception` -> `500 unexpected_error`

## 7. Persistence and Database

EF Core context: `AppDbContext`.

Mappings:

- `Person.Name`: required, max 200.
- `Person.BirthDate`: required, `date`.
- `Transaction.Description`: required, max 400.
- `Transaction.Value`: `decimal(18,2)`.
- `Transaction.Date`: `timestamp without time zone`.
- `Category.Description`: required, max 400.
- `Category.Purpose`: stored as int.

Relationships:

- `Person 1:N Transactions` with `Cascade` delete.
- `Category 1:N Transactions` with `Restrict` delete.

Migrations are present in `src/ControleGastos.Infrastructure/Migrations`.

## 8. Configuration and Secrets

### Connection String
Connection strategy in `Program.cs`:

1. Prefer environment variable `ConnectionStrings__DefaultConnection`.
2. Fallback to config `ConnectionStrings:DefaultConnection`.
3. If available, apply password from `Database:Password` or `DB_PASSWORD`.

### JWT Configuration
Em `appsettings.json` → `Auth:Jwt`:
- `Secret` — chave para HS256 (mín 32 chars)
- `Issuer` — validação de emissor
- `Audience` — validação de audiência
- `AccessTokenMinutes` — duração JWT (default: 15)
- `RefreshTokenDays` — duracao refresh (default: 7)

Em `Program.cs`:
```csharp
TokenValidationParameters:
  - ValidIssuer
  - ValidAudience
  - ValidateIssuerSigningKey
  - ValidateLifetime
  - ClockSkew: 30 segundos (tolerância)
```

### Google OAuth Configuration
Em `appsettings.json` → `Auth:Google`:
- `ClientId` — ID do cliente Google (obtido em console.cloud.google.com)

Validação:
- Prod: Valida assinatura com Google API
- Testes: Fake validator (para testes sem chamar Google)

### Secret Handling

- Local secret should be placed in `src/ControleGastos.API/appsettings.Development.json`.
- Example file: `src/ControleGastos.API/appsettings.Development.example.json`.
- Local development config file is git-ignored.
- **Produção:** Use Azure Key Vault, Secrets Manager, ou variáveis de ambiente

## 9. Runtime Notes

- Swagger is enabled only in `Development`.
- Access Swagger at `/swagger`.
- Root path `/` is not mapped (requesting `/` returns 404 unless explicitly mapped).
- `UseHttpsRedirection` may log warning about HTTPS port when running HTTP-only profile.

## 10. Tests

Test projects:

- `ControleGastos.Domain.Tests`
- `ControleGastos.Application.Tests`
- `ControleGastos.API.IntegrationTests`

Coverage focus:

- Domain entity rules
- Application service rules
- FluentValidation validators
- API integration flows (including error envelope behavior)

Current test volume is approximately 43 test cases.

## 11. Known Scope Decisions

1. Person model keeps `BirthDate` as source of truth.
2. `Age` is calculated at runtime (not persisted as dedicated DB field).
3. API error payload is standardized with custom envelope.
4. **JWT armazenado em memória** (React state) — melhor Performance
5. **RefreshToken em cookie HttpOnly** — proteção XSS
6. **Email normalizado** (trim, lowercase) — evita duplicatas
7. **PasswordHash com PBKDF2** — strong hashing
8. **Person criada automaticamente** — ao fazer login/registro

## 12. Deploy em Produção

### Build para Produção

```bash
# Restore dependencies
dotnet restore

# Build em modo Release (otimizado)
dotnet build -c Release

# Publish (gera output em ./publish)
dotnet publish -c Release -o ./publish
```

### Configuração para Produção

1. **Environment:**
   - Definir `ASPNETCORE_ENVIRONMENT=Production`.
   - Desabilitar Swagger em produção (ou proteger com autenticação).

2. **Variáveis de Ambiente Necessárias:**
   ```env
   # Database
   ConnectionStrings__DefaultConnection=postgresql://user:password@host:5432/controle_gastos
   DB_PASSWORD=sua_senha_segura

   # JWT
   Auth__Jwt__Secret=sua_chave_secreta_com_minimo_32_caracteres
   Auth__Jwt__AccessTokenMinutes=60
   Auth__Jwt__RefreshTokenDays=30

   # OAuth Google (opcional)
   Auth__Google__ClientId=seu_google_client_id
   ```

3. **Dockerfile para Produção:**
   - Multi-stage: SDK .NET 8 → build → Runtime .NET 8 Alpine.
   - Otimizado para tamanho e segurança.
   - Sem dependências de build na imagem runtime.

4. **CORS em Produção:**
   - Configurar apenas origem permitida (frontend domain).
   - Remover `AllowAnyOrigin()`.
   - Exemplo: `policy.WithOrigins("https://seu_dominio.com")`

5. **HTTPS Obrigatório:**
   - Backend deve estar atrás de proxy reverso (Nginx) com SSL/TLS.
   - Let's Encrypt para certificados gratuitos.
   - Habilitar HSTS headers via Nginx.

### Docker Compose (Integração)

O serviço `api` usa:
```yaml
api:
  build:
    context: ./backend
    dockerfile: Dockerfile
  environment:
    ASPNETCORE_ENVIRONMENT: Production
    ConnectionStrings__DefaultConnection: ${CONNECTION_STRING}
    Auth__Jwt__Secret: ${JWT_SECRET}
  depends_on:
    db:
      condition: service_healthy
  networks:
    - app_network
  restart: unless-stopped
```

## 13. Migrations em Produção

```bash
# Aplicar migrations automáticamente no startup:
# (Recomendado para produção simples)
app.MigrateDatabase();  // Em Program.cs

# Ou manual (conexão SSH na VM):
docker-compose exec api dotnet ef database update --project ControleGastos.Infrastructure
```

## 14. Monitoramento e Observabilidade

### Logs

- Logs estruturados via Serilog (recomendação futura).
- Logs em arquivo ou serviço centralizado (Azure Application Insights, etc).
- Rotação de logs automática.

### Health Checks

- Implementar endpoint `/health` para verificações de status.
- Verificar conexão com banco de dados.
- Docker health checks baseados nesse endpoint.

### Métricas

- Prometheus (futura integração).
- Contar requisições, latência, erros.
- Alertas baseados em limiares.

## 15. Segurança em Produção

1. **SQL Injection:** Mitigado por EF Core (LINQ + parameterized queries).
2. **CORS:** Restringido a domínios conhecidos.
3. **JWT Validation:** Chaves secretas fortes (mínimo 32 caracteres).
4. **Rate Limiting:** Considerar adicionar (middleware customizado ou Nginx).
5. **Input Validation:** FluentValidation em todas as DTOs.
6. **Error Handling:** Não expor stack traces em produção (ExceptionHandlingMiddleware).
7. **HTTPS Obrigatório:** Redirecionar HTTP → HTTPS via Nginx.

## 16. Backup e Recuperação

### PostgreSQL (Autonomous DB no Oracle Cloud)

- Backups automáticos gerenciados pelo Oracle (diário, retenção 7 dias).
- Point-in-time recovery disponível.
- Snapshots manuais conforme necessário.

### Backup Local (para desarrollo)

```bash
# Dump do banco
pg_dump -h localhost -U postgres -d controle_gastos > backup.sql

# Restore
psql -h localhost -U postgres -d controle_gastos < backup.sql
```

## 17. Escalabilidade

### Limitações Atuais

- Single instance backend (sem load balancer).
- Banco de dados único (sem replicação).
- Adequado para: pequenas equipes, MVP, prototipagem.

### Futuras Melhorias

- Load balancer (NGINX, HAProxy, ou Azure Load Balancer).
- Database replication e failover.
- Cache distribuído (Redis).
- Message queue (RabbitMQ, Kafka).
- Microserviços / Event-driven architecture.

## 18. Troubleshooting Comum

### Erro: "Npgsql.NpgsqlException: password authentication failed"
**Solução:** Verificar environment variable `DB_PASSWORD` e connection string.

### Erro: "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"
**Solução:** Concorrência detectada. EF Core já trata com retry automático.

### Erro: "CORS policy: No 'Access-Control-Allow-Origin' header"
**Solução:** Adicionar origem frontend em AddCors() no Program.cs.

### Banco não conecta após deploy
**Solução:** 
1. Verificar connection string no console Oracle Cloud.
2. Verificar Network ACL na VCN (porta 5432 aberta).
3. Testar conexão manual: `psql -h <host> -U postgres`.

## 19. Checklist: Pronto para Produção?

- [ ] Autenticação JWT implementada e testada
- [ ] HTTPS configurado com certificado válido
- [ ] Migrations aplicadas ao banco de produção
- [ ] Variáveis de ambiente configuradas (securely)
- [ ] CORS ajustado para domínio específico
- [ ] Swagger desabilitado ou protegido
- [ ] Error handling customizado (sem stack traces)
- [ ] Logs estruturados habilitados
- [ ] Backups automáticos configurados
- [ ] Health checks implementados
- [ ] Testes de carga executados
- [ ] Monitoramento baseado em alertas

## 20. Próximas Melhorias

1. **Autenticação & Authz:** JWT + OAuth Google (infraestrutura pronta).
2. **Observabilidade:** Serilog + Application Insights.
3. **CI/CD:** GitHub Actions com build/test/deploy gates.
4. **API Versioning:** MediaType ou URL-based versioning.
5. **Rate Limiting:** Middleware de rate limit.
6. **Caching:** Redis para cache distribuído.
7. **API Documentation:** Swagger robusto com exemplos.

## 21. Resumo executivo

Backend .NET 8 + ASP.NET Core organizado em camadas (API, Application, Domain, Infrastructure) com validação robusta via FluentValidation, tratamento de erros centralizado, e persistência em PostgreSQL via Entity Framework Core. Arquitetura escalável com domain entities fortemente tipadas e business rules aplicadas na camada de aplicação. Pronto para deploy containerizado em Docker/Kubernetes. Autenticação JWT está em infraestrutura, aguardando implementação de casos de uso de login/refresh.

**Para deploy em Oracle Cloud Always Free:**
- Usar Dockerfile multi-stage com .NET 8 SDK → .NET 8 Runtime Alpine.
- Autonomous Database PostgreSQL (gerenciado, backups automáticos).
- Variáveis de ambiente em .env com connection strings seguras.
- HTTPS via Let's Encrypt renovat automaticamente.
- Migrations aplicadas automaticamente ou manual via docker-compose exec.

## 22. Referências Úteis

- [.NET 8 Docs](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [FluentValidation](https://fluentvalidation.net/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [Oracle Cloud Always Free](https://www.oracle.com/cloud/free/)
