# Controle de Gastos - Frontend

Aplicação web em React + TypeScript para o teste de **controle de gastos residenciais**.

## Tecnologias

- React 19
- TypeScript
- Vite
- Axios
- React Router

## Como executar

1. Instale as dependências:

```bash
npm install
```

2. Configure a URL da API (opcional). Por padrão, a aplicação usa `http://localhost:5034/api`.

```bash
# .env
VITE_API_BASE_URL=http://localhost:5034/api
```

3. Execute em modo desenvolvimento:

```bash
npm run dev
```

## Estrutura principal

- `src/services/apiClient.ts`: cliente HTTP base com `baseURL` centralizada.
- `src/services/personService.ts`: operações de CRUD de Pessoa consumindo `/api/person`.
- `src/services/categoryService.ts`: operações de criação e listagem de Categoria consumindo `/api/category`.
- `src/types/person.ts`: contratos de tipagem (`Person`, `PersonDto`).
- `src/types/category.ts`: contratos de tipagem (`Category`, `CategoryDto`, `CategoryPurpose`).
- `src/pages/people/PeoplePage.tsx`: tela de listagem + formulário de criação/edição + exclusão.
- `src/pages/categories/CategoriesPage.tsx`: tela de listagem + formulário de criação de categorias.

## Funcionalidades já implementadas no frontend

- CRUD completo de **Pessoa**:
  - Criar
  - Listar
  - Editar
  - Excluir
- Cadastro de **Categoria**:
  - Criar
  - Listar
- Validação de formulário alinhada ao backend:
  - Nome obrigatório e máximo de 200 caracteres
  - Data de nascimento obrigatória e anterior à data atual
  - Descrição de categoria obrigatória e máximo de 400 caracteres
  - Finalidade de categoria obrigatória
- Feedbacks de sucesso e erro em português.

## Observações de contrato

- Nomenclatura técnica (métodos e propriedades) mantida em inglês para seguir o backend.
- Textos visíveis ao usuário mantidos em português.
