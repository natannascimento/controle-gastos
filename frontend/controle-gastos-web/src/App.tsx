import { NavLink, Navigate, Route, Routes } from "react-router-dom";
import { PeoplePage } from "./pages/people/PeoplePage";
import { CategoriesPage } from "./pages/categories/CategoriesPage";
import { TransactionsPage } from "./pages/transactions/TransactionsPage";
import { ReportsPage } from "./pages/reports/ReportsPage";
import { AuthPage } from "./pages/auth/AuthPage";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { useAuth } from "./auth/useAuth";
import "./App.css";

function App() {
  const { status, user, logout } = useAuth();

  const isAuthenticated = status === "authenticated";

  return (
    <div>
      {isAuthenticated && (
        <nav className="app-nav">
          <div className="app-nav__links">
            <NavLink
              to="/transactions"
              className={({ isActive }) => (isActive ? "app-nav__link app-nav__link--active" : "app-nav__link")}
            >
              Transações
            </NavLink>
            <NavLink
              to="/people"
              className={({ isActive }) => (isActive ? "app-nav__link app-nav__link--active" : "app-nav__link")}
            >
              Pessoas
            </NavLink>
            <NavLink
              to="/categories"
              className={({ isActive }) => (isActive ? "app-nav__link app-nav__link--active" : "app-nav__link")}
            >
              Categorias
            </NavLink>
            <NavLink
              to="/reports"
              className={({ isActive }) => (isActive ? "app-nav__link app-nav__link--active" : "app-nav__link")}
            >
              Relatórios
            </NavLink>
          </div>

          <div className="app-nav__session">
            <span className="app-nav__user">{user?.name}</span>
            <button type="button" className="app-nav__logout" onClick={() => void logout()}>
              Sair
            </button>
          </div>
        </nav>
      )}

      <Routes>
        <Route path="/" element={<Navigate to={isAuthenticated ? "/reports" : "/auth"} replace />} />
        <Route path="/auth" element={<AuthPage />} />
        <Route
          path="/reports"
          element={
            <ProtectedRoute>
              <ReportsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/transactions"
          element={
            <ProtectedRoute>
              <TransactionsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/people"
          element={
            <ProtectedRoute>
              <PeoplePage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/categories"
          element={
            <ProtectedRoute>
              <CategoriesPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </div>
  );
}

export default App;
