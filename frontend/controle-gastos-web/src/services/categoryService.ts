import { apiClient } from "./apiClient";
import type { Category, CategoryDto } from "../types/category";

const CATEGORY_ENDPOINT = "/category";

export async function getCategories(): Promise<Category[]> {
  const response = await apiClient.get<Category[]>(CATEGORY_ENDPOINT);
  return response.data;
}

export async function createCategory(categoryDto: CategoryDto): Promise<Category> {
  const response = await apiClient.post<Category>(CATEGORY_ENDPOINT, categoryDto);
  return response.data;
}
