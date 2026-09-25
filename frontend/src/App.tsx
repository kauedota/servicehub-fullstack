import { Navigate, Route, Routes } from 'react-router-dom';
import { useAuth } from './auth/AuthContext';
import Layout from './components/Layout';
import LoginPage from './pages/LoginPage';
import HomePage from './pages/HomePage';
import DashboardPage from './pages/DashboardPage';
import ClientesPage from './pages/ClientesPage';
import ProjetosPage from './pages/ProjetosPage';
import ChamadosPage from './pages/ChamadosPage';
import SobrePage from './pages/SobrePage';

// Rotas: /login é pública; o resto exige usuário logado (senão redireciona para /login).
export default function App() {
  const { usuario } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={usuario ? <Navigate to="/" replace /> : <LoginPage />} />
      <Route element={usuario ? <Layout /> : <Navigate to="/login" replace />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/clientes" element={<ClientesPage />} />
        <Route path="/projetos" element={<ProjetosPage />} />
        <Route path="/chamados" element={<ChamadosPage />} />
        <Route path="/sobre" element={<SobrePage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
