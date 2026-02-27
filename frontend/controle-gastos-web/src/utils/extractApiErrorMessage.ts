import axios from "axios";

export function extractApiErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return "Ocorreu um erro inesperado. Tente novamente.";
  }

  const responseData = error.response?.data as
    | { message?: string; title?: string; errors?: Record<string, string[]> }
    | undefined;

  if (responseData?.message) {
    return responseData.message;
  }

  if (responseData?.title) {
    return responseData.title;
  }

  if (responseData?.errors) {
    const firstError = Object.values(responseData.errors).flat()[0];
    if (firstError) {
      return firstError;
    }
  }

  return "Não foi possível processar sua solicitação.";
}
