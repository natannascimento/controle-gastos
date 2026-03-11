import type { ReactElement } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./useAuth";

export function ProtectedRoute({ children }: { children: ReactElement }) {
  const { status } = useAuth();
  const location = useLocation();

  if (status === "loading") {
    return <p style={{ padding: "24px", textAlign: "center" }}>Carregando sessão...</p>;
  }

  if (status === "unauthenticated") {
    return <Navigate to="/auth" replace state={{ from: location.pathname }} />;
  }

  return children;
}
