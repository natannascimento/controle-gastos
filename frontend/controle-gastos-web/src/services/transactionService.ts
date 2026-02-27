import { apiClient } from "./apiClient";
import type { CreateTransactionDto, Transaction } from "../types/transaction";

const TRANSACTION_ENDPOINT = "/transaction";

export async function getTransactions(): Promise<Transaction[]> {
  const response = await apiClient.get<Transaction[]>(TRANSACTION_ENDPOINT);
  return response.data;
}

export async function createTransaction(
  transactionDto: CreateTransactionDto
): Promise<Transaction> {
  const response = await apiClient.post<Transaction>(
    TRANSACTION_ENDPOINT,
    transactionDto
  );
  return response.data;
}
