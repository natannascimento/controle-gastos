import { useEffect, useState } from "react";
import { CategoryPurpose } from "../../types/category";
import {
  getTotalsByCategories,
  getTotalsByPersons,
} from "../../services/reportService";
import type {
  CategoryTotalsResponse,
  PersonTotalsResponse,
  TotalsSummary,
} from "../../types/report";
import { extractApiErrorMessage } from "../../utils/extractApiErrorMessage";
import "./reports-page.css";

function formatCurrency(value: number): string {
  return value.toLocaleString("pt-BR", {
    style: "currency",
    currency: "BRL",
  });
}

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

function SummaryCards({ summary }: { summary: TotalsSummary }) {
  return (
    <div className="reports-summary-grid">
      <article className="reports-summary-card">
        <h4>Total de receitas</h4>
        <p>{formatCurrency(summary.totalIncome)}</p>
      </article>
      <article className="reports-summary-card">
        <h4>Total de despesas</h4>
        <p>{formatCurrency(summary.totalExpense)}</p>
      </article>
      <article className="reports-summary-card">
        <h4>Saldo líquido</h4>
        <p>{formatCurrency(summary.balance)}</p>
      </article>
    </div>
  );
}

export function ReportsPage() {
  const [personTotals, setPersonTotals] = useState<PersonTotalsResponse | null>(
    null
  );
  const [categoryTotals, setCategoryTotals] =
    useState<CategoryTotalsResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function loadReports() {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const [personsData, categoriesData] = await Promise.all([
        getTotalsByPersons(),
        getTotalsByCategories(),
      ]);
      setPersonTotals(personsData);
      setCategoryTotals(categoriesData);
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadReports();
  }, []);

  return (
    <main className="reports-page">
      <header className="reports-page__header">
        <h1>Relatórios</h1>
        <p>
          Acompanhe os totais de receitas, despesas e saldo por pessoa e
          categoria.
        </p>
      </header>

      {errorMessage && <p className="reports-message reports-message--error">{errorMessage}</p>}

      {isLoading ? (
        <section className="reports-page__card">
          <p>Carregando relatórios...</p>
        </section>
      ) : (
        <>
          <section className="reports-page__card">
            <h2>Totais por pessoa</h2>

            {personTotals && personTotals.items.length > 0 ? (
              <>
                <div className="reports-table-wrapper">
                  <table>
                    <thead>
                      <tr>
                        <th>Pessoa</th>
                        <th>Receitas</th>
                        <th>Despesas</th>
                        <th>Saldo</th>
                      </tr>
                    </thead>
                    <tbody>
                      {personTotals.items.map((item) => (
                        <tr key={item.personId}>
                          <td>{item.name}</td>
                          <td>{formatCurrency(item.totalIncome)}</td>
                          <td>{formatCurrency(item.totalExpense)}</td>
                          <td>{formatCurrency(item.balance)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                <h3>Resumo geral</h3>
                <SummaryCards summary={personTotals.summary} />
              </>
            ) : (
              <p>Nenhum dado de pessoa disponível para relatório.</p>
            )}
          </section>

          <section className="reports-page__card">
            <h2>Totais por categoria</h2>

            {categoryTotals && categoryTotals.items.length > 0 ? (
              <>
                <div className="reports-table-wrapper">
                  <table>
                    <thead>
                      <tr>
                        <th>Categoria</th>
                        <th>Finalidade</th>
                        <th>Receitas</th>
                        <th>Despesas</th>
                        <th>Saldo</th>
                      </tr>
                    </thead>
                    <tbody>
                      {categoryTotals.items.map((item) => (
                        <tr key={item.categoryId}>
                          <td>{item.description}</td>
                          <td>{getPurposeLabel(item.purpose)}</td>
                          <td>{formatCurrency(item.totalIncome)}</td>
                          <td>{formatCurrency(item.totalExpense)}</td>
                          <td>{formatCurrency(item.balance)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                <h3>Resumo geral</h3>
                <SummaryCards summary={categoryTotals.summary} />
              </>
            ) : (
              <p>Nenhum dado de categoria disponível para relatório.</p>
            )}
          </section>
        </>
      )}
    </main>
  );
}
