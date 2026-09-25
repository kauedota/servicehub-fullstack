import { useEffect, useState } from 'react';
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import Footer from './Footer';

// Estrutura de todas as páginas logadas: barra superior + conteúdo + rodapé.
// Em telas grandes o menu fica sempre visível;
// em tablet/celular (≤ 900px) ele fica escondido atrás do botão hambúrguer.
export default function Layout() {
  const { usuario, logout } = useAuth();
  const [menuAberto, setMenuAberto] = useState(false);
  const location = useLocation();

  // Fecha o menu e volta ao topo quando o usuário navega para outra página.
  useEffect(() => {
    setMenuAberto(false);
    window.scrollTo(0, 0);
  }, [location.pathname]);

  // Fecha o menu com a tecla Esc.
  useEffect(() => {
    if (!menuAberto) return;
    const aoPressionar = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMenuAberto(false);
    };
    window.addEventListener('keydown', aoPressionar);
    return () => window.removeEventListener('keydown', aoPressionar);
  }, [menuAberto]);

  return (
    <div className="app">
      <header className="topbar">
        {/* Clicar no logo volta para a página inicial */}
        <Link to="/" className="marca" aria-label="Service Hub, ir para a página inicial">
          <span className="marca-icone" aria-hidden="true">⏱</span> Service Hub
        </Link>

        <button
          className={`hamburguer ${menuAberto ? 'aberto' : ''}`}
          aria-label={menuAberto ? 'Fechar menu' : 'Abrir menu'}
          aria-expanded={menuAberto}
          aria-controls="menu-principal"
          onClick={() => setMenuAberto((v) => !v)}
        >
          <span />
          <span />
          <span />
        </button>

        <div id="menu-principal" className={`menu ${menuAberto ? 'aberto' : ''}`}>
          <nav>
            <NavLink to="/" end>Início</NavLink>
            <NavLink to="/dashboard">Dashboard</NavLink>
            <NavLink to="/clientes">Clientes</NavLink>
            <NavLink to="/projetos">Projetos</NavLink>
            <NavLink to="/chamados">Chamados</NavLink>
            <NavLink to="/sobre">Sobre</NavLink>
          </nav>
          <div className="usuario">
            <span>
              {usuario?.nome} <small className="perfil">{usuario?.perfil}</small>
            </span>
            <button className="btn-link" onClick={logout}>Sair</button>
          </div>
        </div>
      </header>

      {/* Fundo escurecido atrás do menu aberto; clicar fora fecha o menu */}
      {menuAberto && <div className="menu-overlay" onClick={() => setMenuAberto(false)} />}

      <main className="conteudo">
        <Outlet />
      </main>

      <Footer />
    </div>
  );
}
