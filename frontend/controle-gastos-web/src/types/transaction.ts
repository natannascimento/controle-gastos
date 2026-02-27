export const TransactionType = {
  Expense: 1,
  Income: 2,
} as const;

export type TransactionType = (typeof TransactionType)[keyof typeof TransactionType];

export interface Transaction {
  id: string;
  description: string;
  value: number;
  date: string;
  personId: string;
  categoryId: string;
  type: TransactionType;
}

export interface CreateTransactionDto {
  personId: string;
  description: string;
  value: number;
  categoryId: string;
  transactionType: TransactionType;
  date: string;
}
