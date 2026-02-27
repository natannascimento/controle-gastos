import { useEffect, useState, type ChangeEvent, type FormEvent } from "react";
import { createCategory, getCategories } from "../../services/categoryService";
import { CategoryPurpose, type Category, type CategoryDto } from "../../types/category";
import { extractApiErrorMessage } from "../../utils/extractApiErrorMessage";
import "./categories-page.css";

interface FormState {
  description: string;
  purpose: CategoryPurpose | "";
}

const INITIAL_FORM_STATE: FormState = {
  description: "",
  purpose: "",
};

function getPurposeLabel(purpose: CategoryPurpose): string {
  switch (purpose) {
    case CategoryPurpose.Expense:
      return "Despesa";
    case CategoryPurpose.Income:
      return "Receita";
    case CategoryPurpose.Both:
      return "Ambas";
    default:
      return "Não informado";
  }
}

function validateForm(form: FormState): string | null {
  if (!form.description.trim()) {
    return "Descrição é obrigatória.";
  }

  if (form.description.trim().length > 400) {
    return "Descrição deve ter no máximo 400 caracteres.";
  }

  if (!form.purpose) {
    return "Finalidade é obrigatória.";
  }

  return null;
}

export function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [form, setForm] = useState<FormState>(INITIAL_FORM_STATE);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  async function loadCategories() {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const categoriesData = await getCategories();
      setCategories(categoriesData);
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadCategories();
  }, []);

  function handleInputChange(event: ChangeEvent<HTMLInputElement | HTMLSelectElement>) {
    const { name, value } = event.target;

    if (name === "purpose") {
      setForm((current) => ({
        ...current,
        purpose: value ? (Number(value) as CategoryPurpose) : "",
      }));
      return;
    }

    setForm((current) => ({ ...current, [name]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    const validationError = validateForm(form);
    if (validationError) {
      setErrorMessage(validationError);
      return;
    }

    const categoryDto: CategoryDto = {
      description: form.description.trim(),
      purpose: form.purpose as CategoryPurpose,
    };

    setIsSaving(true);

    try {
      await createCategory(categoryDto);
      setSuccessMessage("Categoria cadastrada com sucesso.");
      setForm(INITIAL_FORM_STATE);
      await loadCategories();
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <main className="categories-page">
      <header className="categories-page__header">
        <h1>Cadastro de Categorias</h1>
        <p>Cadastre as categorias usadas para classificar receitas e despesas.</p>
      </header>

      <section className="categories-page__card">
        <h2>Nova categoria</h2>

        <form className="categories-form" onSubmit={handleSubmit}>
          <label htmlFor="description">Descrição</label>
          <input
            id="description"
            name="description"
            type="text"
            maxLength={400}
            value={form.description}
            onChange={handleInputChange}
            placeholder="Ex.: Alimentação"
          />

          <label htmlFor="purpose">Finalidade</label>
          <select
            id="purpose"
            name="purpose"
            value={form.purpose}
            onChange={handleInputChange}
          >
            <option value="">Selecione</option>
            <option value={CategoryPurpose.Expense}>Despesa</option>
            <option value={CategoryPurpose.Income}>Receita</option>
            <option value={CategoryPurpose.Both}>Ambas</option>
          </select>

          <div className="categories-form__actions">
            <button type="submit" disabled={isSaving}>
              {isSaving ? "Salvando..." : "Cadastrar categoria"}
            </button>
          </div>
        </form>

        {errorMessage && (
          <p className="categories-message categories-message--error">{errorMessage}</p>
        )}
        {successMessage && (
          <p className="categories-message categories-message--success">{successMessage}</p>
        )}
      </section>

      <section className="categories-page__card">
        <h2>Categorias cadastradas</h2>

        {isLoading ? (
          <p>Carregando categorias...</p>
        ) : categories.length === 0 ? (
          <p>Nenhuma categoria cadastrada até o momento.</p>
        ) : (
          <div className="categories-table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Descrição</th>
                  <th>Finalidade</th>
                </tr>
              </thead>
              <tbody>
                {categories.map((category) => (
                  <tr key={category.id}>
                    <td>{category.description}</td>
                    <td>{getPurposeLabel(category.purpose)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </main>
  );
}
