import { useEffect, useState } from 'react';
import { dashboardApi } from '../api/services';
import type { Dashboard } from '../api/types';
import { useToast } from '../components/Toast';
import { rotulo } from '../utils/format';

export default function DashboardPage() {
  const toast = useToast();
  const [dados, setDados] = useState<Dashboard | null>(null);
  const [falhou, setFalhou] = useState(false);

  useEffect(() => {
    dashboardApi
      .obter()
      .then(setDados)
      .catch((e: Error) => {
        setFalhou(true);
        toast.erro(e.message);
      });
  }, [toast]);

  if (falhou) return <p className="muted">Não foi possível carregar o dashboard.</p>;
  if (!dados) return <p className="muted">Carregando...</p>;

  const indicadores = [
    { titulo: 'Clientes', valor: dados.totalClientes },
    { titulo: 'Projetos', valor: dados.totalProjetos },
    { titulo: 'Chamados abertos', valor: dados.chamadosAbertos },
    { titulo: 'Em andamento', valor: dados.chamadosEmAndamento },
    { titulo: 'Concluídos', valor: dados.chamadosConcluidos },
  ];

  const maximo = Math.max(1, ...dados.chamadosPorPrioridade.map((p) => p.total));

  return (
    <>
      <h2>Dashboard</h2>

      <section className="indicadores">
        {indicadores.map((i) => (
          <div key={i.titulo} className="card indicador">
            <span className="indicador-titulo">{i.titulo}</span>
            <strong className="indicador-valor">{i.valor}</strong>
          </div>
        ))}
      </section>

      <section className="card">
        <h3>Chamados por prioridade</h3>
        <p className="muted">Total de chamados em cada prioridade (entre parênteses, os ainda não resolvidos).</p>
        <div className="barras">
          {dados.chamadosPorPrioridade.map((p) => (
            <div
              key={p.prioridade}
              className="barra-linha"
              title={`${rotulo(p.prioridade)}: ${p.total} chamado(s), ${p.emAberto} em aberto`}
            >
              <span className="barra-rotulo">{rotulo(p.prioridade)}</span>
              <div className="barra-trilho">
                <div className="barra" style={{ width: `${(p.total / maximo) * 100}%` }} />
              </div>
              <span className="barra-valor">
                {p.total} <small className="muted">({p.emAberto})</small>
              </span>
            </div>
          ))}
          {dados.chamadosPorPrioridade.length === 0 && <p className="muted">Nenhum chamado cadastrado.</p>}
        </div>
      </section>
    </>
  );
}
