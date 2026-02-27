export const CategoryPurpose = {
  Expense: 1,
  Income: 2,
  Both: 3,
} as const;

export type CategoryPurpose = (typeof CategoryPurpose)[keyof typeof CategoryPurpose];

export interface Category {
  id: string;
  description: string;
  purpose: CategoryPurpose;
}

export interface CategoryDto {
  description: string;
  purpose: CategoryPurpose;
}
