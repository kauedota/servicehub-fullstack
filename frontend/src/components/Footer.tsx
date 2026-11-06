const TECNOLOGIAS = ['.NET 8', 'React', 'TypeScript', 'SQL Server'];

export default function Footer() {
  return (
    <footer className="rodape">
      <div className="rodape-conteudo">
        <div>
          <strong>⏱ Service Hub</strong>
          <p className="rodape-texto">Gestão de clientes, projetos e chamados.</p>
        </div>
        <ul className="rodape-tags" aria-label="Tecnologias utilizadas">
          {TECNOLOGIAS.map((t) => <li key={t}>{t}</li>)}
        </ul>
      </div>
      <div className="rodape-base">
        <span>© {new Date().getFullYear()} Service Hub. Projeto de portfólio.</span>
        <span className="rodape-externos">
          <a href="https://github.com/kauedota/servicehub-fullstack" target="_blank" rel="noopener noreferrer">GitHub</a>
          <a href="https://www.linkedin.com/in/kaue-dota/" target="_blank" rel="noopener noreferrer">LinkedIn</a>
        </span>
      </div>
    </footer>
  );
}
