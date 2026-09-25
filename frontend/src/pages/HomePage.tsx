import { Link } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import HeroIlustracao from '../components/HeroIlustracao';

const ATALHOS = [
  { rota: '/dashboard', icone: '📊', titulo: 'Dashboard', texto: 'Indicadores de clientes, projetos e chamados por prioridade.' },
  { rota: '/clientes', icone: '🏢', titulo: 'Clientes', texto: 'Cadastro de clientes de varejo, saúde, finanças e outros setores.' },
  { rota: '/projetos', icone: '🗂️', titulo: 'Projetos', texto: 'Projetos de cada cliente, com status e responsável.' },
  { rota: '/chamados', icone: '🎫', titulo: 'Chamados', texto: 'Abertura, priorização, atribuição e histórico de chamados.' },
];

// Página inicial: banner com ilustração + atalhos para as áreas do sistema.
export default function HomePage() {
  const { usuario } = useAuth();
  const primeiroNome = usuario?.nome.split(' ')[0] ?? '';

  return (
    <>
      <section className="hero">
        <div className="hero-texto">
          <span className="hero-saudacao">Olá, {primeiroNome} 👋</span>
          <h1>Clientes, projetos e chamados em um só lugar</h1>
          <p>
            O Service Hub centraliza a sustentação dos seus clientes: acompanhe projetos, priorize chamados
            e veja o que está acontecendo em tempo real.
          </p>
          <div className="hero-acoes">
            <Link to="/chamados" className="btn btn-claro">Abrir chamado</Link>
            <Link to="/dashboard" className="btn btn-contorno">Ver dashboard</Link>
          </div>
        </div>
        <div className="hero-imagem">
          <HeroIlustracao />
        </div>
      </section>

      <section className="atalhos">
        {ATALHOS.map((a) => (
          <Link key={a.rota} to={a.rota} className="card atalho">
            <span className="atalho-icone" aria-hidden="true">{a.icone}</span>
            <strong>{a.titulo}</strong>
            <span className="muted">{a.texto}</span>
            <span className="atalho-seta" aria-hidden="true">→</span>
          </Link>
        ))}
      </section>
    </>
  );
}
