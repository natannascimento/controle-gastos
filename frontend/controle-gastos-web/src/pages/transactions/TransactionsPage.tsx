import { useEffect, useMemo, useState, type ChangeEvent, type FormEvent } from "react";
import { getPeople } from "../../services/personService";
import { getCategories } from "../../services/categoryService";
import { createTransaction, getTransactions } from "../../services/transactionService";
import { CategoryPurpose, type Category } from "../../types/category";
import type { Person } from "../../types/person";
import {
  TransactionType,
  type CreateTransactionDto,
  type Transaction,
} from "../../types/transaction";
import { extractApiErrorMessage } from "../../utils/extractApiErrorMessage";
import "./transactions-page.css";

interface FormState {
  personId: string;
  description: string;
  value: string;
  categoryId: string;
  transactionType: TransactionType | "";
  date: string;
}

const INITIAL_FORM_STATE: FormState = {
  personId: "",
  description: "",
  value: "",
  categoryId: "",
  transactionType: "",
  date: new Date().toISOString().slice(0, 10),
};

function getTransactionTypeLabel(type: TransactionType): string {
  return type === TransactionType.Expense ? "Despesa" : "Receita";
}

function getCategoryPurposeLabel(purpose: CategoryPurpose): string {
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

function canUseCategoryForType(category: Category, transactionType: TransactionType): boolean {
  if (category.purpose === CategoryPurpose.Both) {
    return true;
  }

  if (category.purpose === CategoryPurpose.Expense) {
    return transactionType === TransactionType.Expense;
  }

  return transactionType === TransactionType.Income;
}

function validateForm(
  form: FormState,
  selectedPerson: Person | undefined,
  availableCategories: Category[]
): string | null {
  if (!form.personId) {
    return "Pessoa é obrigatória.";
  }

  if (!form.description.trim()) {
    return "Descrição é obrigatória.";
  }

  if (form.description.trim().length > 400) {
    return "Descrição deve ter no máximo 400 caracteres.";
  }

  if (!form.value || Number(form.value) <= 0) {
    return "Valor deve ser maior que zero.";
  }

  if (!form.transactionType) {
    return "Tipo da transação é obrigatório.";
  }

  if (!form.categoryId) {
    return "Categoria é obrigatória.";
  }

  if (!form.date) {
    return "Data é obrigatória.";
  }

  if (selectedPerson && selectedPerson.age < 18 && form.transactionType === TransactionType.Income) {
    return "Menor de idade pode registrar apenas despesa.";
  }

  const categorySelected = availableCategories.find((category) => category.id === form.categoryId);
  if (!categorySelected) {
    return "Categoria inválida para o tipo de transação.";
  }

  return null;
}

export function TransactionsPage() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [people, setPeople] = useState<Person[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [form, setForm] = useState<FormState>(INITIAL_FORM_STATE);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const selectedPerson = useMemo(
    () => people.find((person) => person.id === form.personId),
    [people, form.personId]
  );

  const availableCategories = useMemo(() => {
    if (!form.transactionType) {
      return categories;
    }

    return categories.filter((category) =>
      canUseCategoryForType(category, form.transactionType as TransactionType)
    );
  }, [categories, form.transactionType]);

  async function loadPageData() {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const [transactionsData, peopleData, categoriesData] = await Promise.all([
        getTransactions(),
        getPeople(),
        getCategories(),
      ]);
      setTransactions(transactionsData);
      setPeople(peopleData);
      setCategories(categoriesData);
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }

  async function loadTransactions() {
    try {
      const transactionsData = await getTransactions();
      setTransactions(transactionsData);
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    }
  }

  useEffect(() => {
    void loadPageData();
  }, []);

  function handleInputChange(event: ChangeEvent<HTMLInputElement | HTMLSelectElement>) {
    const { name, value } = event.target;

    if (name === "transactionType") {
      setForm((current) => {
        const nextType = value ? (Number(value) as TransactionType) : "";
        const isCurrentCategoryValid =
          current.categoryId &&
          nextType &&
          categories.some(
            (category) =>
              category.id === current.categoryId &&
              canUseCategoryForType(category, nextType as TransactionType)
          );

        return {
          ...current,
          transactionType: nextType,
          categoryId: isCurrentCategoryValid ? current.categoryId : "",
        };
      });
      return;
    }

    setForm((current) => ({ ...current, [name]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    const validationError = validateForm(form, selectedPerson, availableCategories);
    if (validationError) {
      setErrorMessage(validationError);
      return;
    }

    const transactionDto: CreateTransactionDto = {
      personId: form.personId,
      description: form.description.trim(),
      value: Number(form.value),
      categoryId: form.categoryId,
      transactionType: form.transactionType as TransactionType,
      // Envia sem timezone para compatibilizar com "timestamp without time zone" no PostgreSQL.
      date: `${form.date}T00:00:00`,
    };

    setIsSaving(true);

    try {
      await createTransaction(transactionDto);
      setSuccessMessage("Transação cadastrada com sucesso.");
      setForm({
        ...INITIAL_FORM_STATE,
        personId: form.personId,
      });
      await loadTransactions();
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    } finally {
      setIsSaving(false);
    }
  }

  function getPersonNameById(personId: string): string {
    const person = people.find((item) => item.id === personId);
    return person?.name ?? "Pessoa não encontrada";
  }

  function getCategoryDescriptionById(categoryId: string): string {
    const category = categories.find((item) => item.id === categoryId);
    return category?.description ?? "Categoria não encontrada";
  }

  return (
    <main className="transactions-page">
      <header className="transactions-page__header">
        <h1>Transações</h1>
        <p>Registre despesas e receitas para cada pessoa cadastrada.</p>
      </header>

      <section className="transactions-page__card">
        <h2>Nova transação</h2>

        <form className="transactions-form" onSubmit={handleSubmit}>
          <label htmlFor="personId">Pessoa</label>
          <select
            id="personId"
            name="personId"
            value={form.personId}
            onChange={handleInputChange}
          >
            <option value="">Selecione</option>
            {people.map((person) => (
              <option key={person.id} value={person.id}>
                {person.name} ({person.age} anos)
              </option>
            ))}
          </select>

          <label htmlFor="description">Descrição</label>
          <input
            id="description"
            name="description"
            type="text"
            maxLength={400}
            value={form.description}
            onChange={handleInputChange}
            placeholder="Ex.: Mercado do mês"
          />

          <label htmlFor="value">Valor</label>
          <input
            id="value"
            name="value"
            type="number"
            min="0.01"
            step="0.01"
            value={form.value}
            onChange={handleInputChange}
            placeholder="0,00"
          />

          <label htmlFor="transactionType">Tipo</label>
          <select
            id="transactionType"
            name="transactionType"
            value={form.transactionType}
            onChange={handleInputChange}
          >
            <option value="">Selecione</option>
            <option value={TransactionType.Expense}>Despesa</option>
            <option
              value={TransactionType.Income}
              disabled={selectedPerson ? selectedPerson.age < 18 : false}
            >
              Receita
            </option>
          </select>

          <label htmlFor="categoryId">Categoria</label>
          <select
            id="categoryId"
            name="categoryId"
            value={form.categoryId}
            onChange={handleInputChange}
          >
            <option value="">Selecione</option>
            {availableCategories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.description} ({getCategoryPurposeLabel(category.purpose)})
              </option>
            ))}
          </select>

          <label htmlFor="date">Data</label>
          <input
            id="date"
            name="date"
            type="date"
            value={form.date}
            onChange={handleInputChange}
          />

          <div className="transactions-form__actions">
            <button type="submit" disabled={isSaving}>
              {isSaving ? "Salvando..." : "Cadastrar transação"}
            </button>
          </div>
        </form>

        {errorMessage && (
          <p className="transactions-message transactions-message--error">{errorMessage}</p>
        )}
        {successMessage && (
          <p className="transactions-message transactions-message--success">{successMessage}</p>
        )}
      </section>

      <section className="transactions-page__card">
        <h2>Transações cadastradas</h2>

        {isLoading ? (
          <p>Carregando transações...</p>
        ) : transactions.length === 0 ? (
          <p>Nenhuma transação cadastrada até o momento.</p>
        ) : (
          <div className="transactions-table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Descrição</th>
                  <th>Pessoa</th>
                  <th>Categoria</th>
                  <th>Tipo</th>
                  <th>Valor</th>
                  <th>Data</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map((transaction) => (
                  <tr key={transaction.id}>
                    <td>{transaction.description}</td>
                    <td>{getPersonNameById(transaction.personId)}</td>
                    <td>{getCategoryDescriptionById(transaction.categoryId)}</td>
                    <td>{getTransactionTypeLabel(transaction.type)}</td>
                    <td>
                      {transaction.value.toLocaleString("pt-BR", {
                        style: "currency",
                        currency: "BRL",
                      })}
                    </td>
                    <td>{new Date(transaction.date).toLocaleDateString("pt-BR")}</td>
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
