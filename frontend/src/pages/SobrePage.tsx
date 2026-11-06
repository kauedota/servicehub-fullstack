import { useEffect, useState } from 'react';

type StatusApi = 'verificando' | 'online' | 'offline';

const STACK = [
  { area: 'Backend', itens: ['C# / .NET 8', 'ASP.NET Core Web API', 'EF Core', 'Dapper', 'ADO.NET', 'JWT', 'Swagger'] },
  { area: 'Frontend', itens: ['React 18', 'TypeScript', 'Vite', 'CSS responsivo'] },
  { area: 'Banco de dados', itens: ['SQL Server 2022', 'T-SQL', 'Stored procedure', 'Índices e FKs'] },
  { area: 'Infra e qualidade', itens: ['Docker', 'Docker Compose', 'Nginx', 'xUnit + Moq', 'CI/CD (exemplo)', 'Health check'] },
];

// Página "Sobre": o que é o sistema, tecnologias, arquitetura e status da API ao vivo.
export default function SobrePage() {
  const [statusApi, setStatusApi] = useState<StatusApi>('verificando');

  // GET /health é público (não precisa de token). Mostra se a API e o banco estão respondendo.
  useEffect(() => {
    fetch('/health')
      .then((r) => setStatusApi(r.ok ? 'online' : 'offline'))
      .catch(() => setStatusApi('offline'));
  }, []);

  return (
    <>
      <h2>Sobre o Service Hub</h2>

      <section className="card sobre-intro">
        <p>
          O <strong>Service Hub</strong> é um sistema de gestão de <strong>clientes, projetos e chamados</strong>,
          inspirado no dia a dia de uma software house que atende empresas de varejo, saúde e finanças.
          Ele permite cadastrar clientes, acompanhar os projetos de cada um, abrir e priorizar chamados,
          atribuir responsáveis e manter um histórico de auditoria de cada alteração.
        </p>
        <p className="muted pequeno">
          Os nomes de empresas e os dados de exemplo são fictícios.
        </p>
        <div className={`status-api status-${statusApi}`} role="status">
          <span className="status-ponto" aria-hidden="true" />
          {statusApi === 'verificando' && 'Verificando API...'}
          {statusApi === 'online' && 'API e banco de dados online'}
          {statusApi === 'offline' && 'API indisponível'}
          <code>GET /health</code>
        </div>
      </section>

      <section className="stack">
        {STACK.map((s) => (
          <div key={s.area} className="card">
            <h3>{s.area}</h3>
            <ul className="tags">
              {s.itens.map((i) => <li key={i}>{i}</li>)}
            </ul>
          </div>
        ))}
      </section>

      <section className="card">
        <h3>Como uma requisição percorre o sistema</h3>
        <ol className="fluxo">
          <li><strong>React</strong><span>tela envia HTTP + JWT</span></li>
          <li><strong>Controller</strong><span>recebe e valida</span></li>
          <li><strong>Service</strong><span>regras de negócio</span></li>
          <li><strong>Repository</strong><span>EF Core · Dapper · ADO.NET</span></li>
          <li><strong>SQL Server</strong><span>dados</span></li>
        </ol>
      </section>
    </>
  );
}
