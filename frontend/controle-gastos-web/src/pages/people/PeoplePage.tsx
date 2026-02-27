import { useEffect, useMemo, useState, type ChangeEvent, type FormEvent } from "react";
import {
  createPerson,
  deletePerson,
  getPeople,
  updatePerson,
} from "../../services/personService";
import type { Person, PersonDto } from "../../types/person";
import { extractApiErrorMessage } from "../../utils/extractApiErrorMessage";
import "./people-page.css";

interface FormState {
  name: string;
  birthDate: string;
}

const INITIAL_FORM_STATE: FormState = {
  name: "",
  birthDate: "",
};

function normalizeBirthDate(dateValue: string): string {
  if (!dateValue) {
    return "";
  }

  return new Date(dateValue).toISOString();
}

function getBirthDateInputValue(dateValue: string): string {
  if (!dateValue) {
    return "";
  }

  return dateValue.slice(0, 10);
}

function validateForm(form: FormState): string | null {
  if (!form.name.trim()) {
    return "Nome é obrigatório.";
  }

  if (form.name.trim().length > 200) {
    return "Nome deve ter no máximo 200 caracteres.";
  }

  if (!form.birthDate) {
    return "Data de nascimento é obrigatória.";
  }

  const birthDate = new Date(form.birthDate);
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  if (birthDate >= today) {
    return "A data de nascimento deve ser anterior à data atual.";
  }

  return null;
}

export function PeoplePage() {
  const [people, setPeople] = useState<Person[]>([]);
  const [form, setForm] = useState<FormState>(INITIAL_FORM_STATE);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const isEditing = useMemo(() => editingId !== null, [editingId]);

  async function loadPeople() {
    // Fonte única de carregamento para manter a lista sempre sincronizada após cada operação CRUD.
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const peopleData = await getPeople();
      setPeople(peopleData);
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadPeople();
  }, []);

  function resetForm() {
    setForm(INITIAL_FORM_STATE);
    setEditingId(null);
  }

  function handleInputChange(event: ChangeEvent<HTMLInputElement>): void {
    const { name, value } = event.target;
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

    const personDto: PersonDto = {
      name: form.name.trim(),
      birthDate: normalizeBirthDate(form.birthDate),
    };

    setIsSaving(true);

    try {
      // O mesmo formulário atende criação e edição para reduzir duplicação de regras.
      if (editingId) {
        await updatePerson(editingId, personDto);
        setSuccessMessage("Pessoa atualizada com sucesso.");
      } else {
        await createPerson(personDto);
        setSuccessMessage("Pessoa cadastrada com sucesso.");
      }

      resetForm();
      await loadPeople();
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    } finally {
      setIsSaving(false);
    }
  }

  function handleEdit(person: Person) {
    setEditingId(person.id);
    setForm({
      name: person.name,
      birthDate: getBirthDateInputValue(person.birthDate),
    });
    setErrorMessage(null);
    setSuccessMessage(null);
  }

  async function handleDelete(person: Person) {
    const shouldDelete = window.confirm(
      `Deseja realmente excluir ${person.name}? Essa ação também remove as transações da pessoa.`
    );

    if (!shouldDelete) {
      return;
    }

    setErrorMessage(null);
    setSuccessMessage(null);

    try {
      await deletePerson(person.id);
      setSuccessMessage("Pessoa excluída com sucesso.");

      if (editingId === person.id) {
        resetForm();
      }

      await loadPeople();
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    }
  }

  return (
    <main className="people-page">
      <header className="people-page__header">
        <h1>Cadastro de Pessoas</h1>
        <p>
          Gerencie pessoas para associar receitas e despesas no controle de
          gastos.
        </p>
      </header>

      <section className="people-page__card">
        <h2>{isEditing ? "Editar pessoa" : "Nova pessoa"}</h2>

        <form className="people-form" onSubmit={handleSubmit}>
          <label htmlFor="name">Nome</label>
          <input
            id="name"
            name="name"
            type="text"
            maxLength={200}
            value={form.name}
            onChange={handleInputChange}
            placeholder="Ex.: Maria Silva"
          />

          <label htmlFor="birthDate">Data de nascimento</label>
          <input
            id="birthDate"
            name="birthDate"
            type="date"
            value={form.birthDate}
            onChange={handleInputChange}
          />

          <div className="people-form__actions">
            <button type="submit" disabled={isSaving}>
              {isSaving
                ? "Salvando..."
                : isEditing
                  ? "Salvar alterações"
                  : "Cadastrar pessoa"}
            </button>

            {isEditing && (
              <button
                type="button"
                className="button-secondary"
                onClick={resetForm}
              >
                Cancelar edição
              </button>
            )}
          </div>
        </form>

        {errorMessage && <p className="message message--error">{errorMessage}</p>}
        {successMessage && (
          <p className="message message--success">{successMessage}</p>
        )}
      </section>

      <section className="people-page__card">
        <h2>Pessoas cadastradas</h2>

        {isLoading ? (
          <p>Carregando pessoas...</p>
        ) : people.length === 0 ? (
          <p>Nenhuma pessoa cadastrada até o momento.</p>
        ) : (
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Data de nascimento</th>
                  <th>Idade</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {people.map((person) => (
                  <tr key={person.id}>
                    <td>{person.name}</td>
                    <td>
                      {new Date(person.birthDate).toLocaleDateString("pt-BR")}
                    </td>
                    <td>{person.age}</td>
                    <td className="table-actions">
                      <button
                        type="button"
                        className="button-secondary"
                        onClick={() => handleEdit(person)}
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        className="button-danger"
                        onClick={() => void handleDelete(person)}
                      >
                        Excluir
                      </button>
                    </td>
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
