import { NavLink, Navigate, Route, Routes } from "react-router-dom";
import { PeoplePage } from "./pages/people/PeoplePage";
import { CategoriesPage } from "./pages/categories/CategoriesPage";
import "./App.css";

function App() {
  return (
    <div>
      <nav className="app-nav">
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
      </nav>

      <Routes>
        <Route path="/" element={<Navigate to="/people" replace />} />
        <Route path="/people" element={<PeoplePage />} />
        <Route path="/categories" element={<CategoriesPage />} />
      </Routes>
    </div>
  );
}

export default App;
