import { Routes, Route } from "react-router-dom";
import { PeoplePage } from "./pages/people/PeoplePage";

function App() {
  return (
    <Routes>
      <Route path="/" element={<PeoplePage />} />
    </Routes>
  );
}

export default App;
