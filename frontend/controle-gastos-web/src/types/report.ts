import type { CategoryPurpose } from "./category";

export interface TotalsSummary {
  totalIncome: number;
  totalExpense: number;
  balance: number;
}

export interface PersonTotalsItem {
  personId: string;
  name: string;
  totalIncome: number;
  totalExpense: number;
  balance: number;
}

export interface PersonTotalsResponse {
  items: PersonTotalsItem[];
  summary: TotalsSummary;
}

export interface CategoryTotalsItem {
  categoryId: string;
  description: string;
  purpose: CategoryPurpose;
  totalIncome: number;
  totalExpense: number;
  balance: number;
}

export interface CategoryTotalsResponse {
  items: CategoryTotalsItem[];
  summary: TotalsSummary;
}
