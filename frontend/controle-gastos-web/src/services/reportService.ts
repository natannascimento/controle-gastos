import { apiClient } from "./apiClient";
import type {
  CategoryTotalsResponse,
  PersonTotalsResponse,
} from "../types/report";

const TOTALS_ENDPOINT = "/totals";

export async function getTotalsByPersons(): Promise<PersonTotalsResponse> {
  const response = await apiClient.get<PersonTotalsResponse>(
    `${TOTALS_ENDPOINT}/persons`
  );
  return response.data;
}

export async function getTotalsByCategories(): Promise<CategoryTotalsResponse> {
  const response = await apiClient.get<CategoryTotalsResponse>(
    `${TOTALS_ENDPOINT}/categories`
  );
  return response.data;
}
