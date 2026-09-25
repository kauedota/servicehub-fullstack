import { Link } from 'react-router-dom';

export default function Footer() {
  return (
    <footer className="rodape">
      <div className="rodape-conteudo">
        <div>
          <strong>⏱ Timeware Service Hub</strong>
          <p className="rodape-texto">Gestão de clientes, projetos e chamados.</p>
        </div>
        <nav className="rodape-links" aria-label="Links do rodapé">
          <Link to="/">Início</Link>
          <Link to="/dashboard">Dashboard</Link>
          <Link to="/chamados">Chamados</Link>
          <Link to="/sobre">Sobre</Link>
        </nav>
      </div>
      <div className="rodape-base">
        © {new Date().getFullYear()} Timeware Service Hub · .NET 8 + React
      </div>
    </footer>
  );
}
