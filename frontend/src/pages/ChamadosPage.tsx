import { useEffect, useState, type FormEvent } from 'react';
import { chamadosApi, clientesApi, projetosApi, usuariosApi } from '../api/services';
import {
  PRIORIDADES, STATUS_CHAMADO,
  type Auditoria, type Chamado, type ChamadoFiltro, type Cliente, type Prioridade, type Projeto,
  type StatusChamado, type Usuario,
} from '../api/types';
import { useToast } from '../components/Toast';
import { formatarData, rotulo } from '../utils/format';

interface FormChamado {
  titulo: string;
  descricao: string;
  clienteId: string;
  projetoId: string;
  responsavelId: string;
  prioridade: Prioridade;
}

const FORM_VAZIO: FormChamado = { titulo: '', descricao: '', clienteId: '', projetoId: '', responsavelId: '', prioridade: 'Media' };
const idOuNulo = (v: string) => (v ? Number(v) : null);

export default function ChamadosPage() {
  const toast = useToast();
  const [chamados, setChamados] = useState<Chamado[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [projetos, setProjetos] = useState<Projeto[]>([]);
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [filtro, setFiltro] = useState<ChamadoFiltro>({ status: '', prioridade: '', clienteId: '' });
  const [form, setForm] = useState<FormChamado>(FORM_VAZIO);
  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [historico, setHistorico] = useState<{ chamado: Chamado; itens: Auditoria[] } | null>(null);

  useEffect(() => {
    Promise.all([clientesApi.listar(), projetosApi.listar(), usuariosApi.listar()])
      .then(([c, p, u]) => { setClientes(c); setProjetos(p); setUsuarios(u); })
      .catch((e: Error) => toast.erro(e.message));
  }, [toast]);

  // Recarrega a lista sempre que um filtro muda (GET /api/chamados?status=...&prioridade=...)
  useEffect(() => {
    chamadosApi.listar(filtro).then(setChamados).catch((e: Error) => toast.erro(e.message));
  }, [filtro, toast]);

  const recarregar = async () => setChamados(await chamadosApi.listar(filtro));
  const projetosDoCliente = projetos.filter((p) => String(p.clienteId) === form.clienteId);

  async function salvar(e: FormEvent) {
    e.preventDefault();
    const dados = {
      titulo: form.titulo,
      descricao: form.descricao,
      projetoId: idOuNulo(form.projetoId),
      responsavelId: idOuNulo(form.responsavelId),
      prioridade: form.prioridade,
    };
    try {
      if (editandoId) {
        await chamadosApi.atualizar(editandoId, dados);                                           // PUT
        toast.sucesso(`Chamado #${editandoId} atualizado.`);
      } else {
        const novo = await chamadosApi.criar({ ...dados, clienteId: Number(form.clienteId) });   // POST
        toast.sucesso(`Chamado #${novo.id} aberto.`);
      }
      setForm(FORM_VAZIO);
      setEditandoId(null);
      await recarregar();
    } catch (e) {
      toast.erro((e as Error).message);
    }
  }

  function editar(c: Chamado) {
    setEditandoId(c.id);
    setForm({
      titulo: c.titulo,
      descricao: c.descricao,
      clienteId: String(c.clienteId),
      projetoId: c.projetoId ? String(c.projetoId) : '',
      responsavelId: c.responsavelId ? String(c.responsavelId) : '',
      prioridade: c.prioridade,
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  async function alterarStatus(c: Chamado, status: StatusChamado) {
    try {
      const atualizado = await chamadosApi.alterarStatus(c.id, status);                          // PATCH
      setChamados((lista) => lista.map((x) => (x.id === c.id ? atualizado : x)));
      toast.sucesso(`Chamado #${c.id}: ${rotulo(c.status)} → ${rotulo(status)}.`);
      if (historico?.chamado.id === c.id) await verHistorico(atualizado);
    } catch (e) {
      toast.erro((e as Error).message);                                                       // ex.: chamado cancelado
    }
  }

  async function verHistorico(c: Chamado) {
    try {
      setHistorico({ chamado: c, itens: await chamadosApi.historico(c.id) });
    } catch (e) {
      toast.erro((e as Error).message);
    }
  }

  return (
    <>
      <h2>Chamados</h2>

      <form className="card form-grid" onSubmit={salvar}>
        <h3 className="span-todo">{editandoId ? `Editar chamado #${editandoId}` : 'Novo chamado'}</h3>
        <label className="span-2">
          Título
          <input value={form.titulo} onChange={(e) => setForm({ ...form, titulo: e.target.value })} required />
        </label>
        <label>
          Prioridade
          <select value={form.prioridade} onChange={(e) => setForm({ ...form, prioridade: e.target.value as Prioridade })}>
            {PRIORIDADES.map((p) => <option key={p} value={p}>{rotulo(p)}</option>)}
          </select>
        </label>
        <label>
          Cliente
          <select
            value={form.clienteId}
            disabled={editandoId !== null}
            onChange={(e) => setForm({ ...form, clienteId: e.target.value, projetoId: '' })}
            required
          >
            <option value="">Selecione...</option>
            {clientes.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
          </select>
        </label>
        <label>
          Projeto (opcional)
          <select value={form.projetoId} onChange={(e) => setForm({ ...form, projetoId: e.target.value })}>
            <option value="">Nenhum</option>
            {projetosDoCliente.map((p) => <option key={p.id} value={p.id}>{p.nome}</option>)}
          </select>
        </label>
        <label>
          Responsável
          <select value={form.responsavelId} onChange={(e) => setForm({ ...form, responsavelId: e.target.value })}>
            <option value="">Não atribuído</option>
            {usuarios.map((u) => <option key={u.id} value={u.id}>{u.nome}</option>)}
          </select>
        </label>
        <label className="span-todo">
          Descrição
          <textarea rows={3} value={form.descricao} onChange={(e) => setForm({ ...form, descricao: e.target.value })} required />
        </label>
        <div className="acoes span-todo">
          <button className="btn">{editandoId ? 'Salvar alterações' : 'Abrir chamado'}</button>
          {editandoId && (
            <button type="button" className="btn btn-secundario" onClick={() => { setEditandoId(null); setForm(FORM_VAZIO); }}>
              Cancelar
            </button>
          )}
        </div>
      </form>

      <div className="card">
        <div className="barra-busca filtros">
          <select aria-label="Filtrar por status" value={filtro.status} onChange={(e) => setFiltro({ ...filtro, status: e.target.value as StatusChamado | '' })}>
            <option value="">Todos os status</option>
            {STATUS_CHAMADO.map((s) => <option key={s} value={s}>{rotulo(s)}</option>)}
          </select>
          <select aria-label="Filtrar por prioridade" value={filtro.prioridade} onChange={(e) => setFiltro({ ...filtro, prioridade: e.target.value as Prioridade | '' })}>
            <option value="">Todas as prioridades</option>
            {PRIORIDADES.map((p) => <option key={p} value={p}>{rotulo(p)}</option>)}
          </select>
          <select aria-label="Filtrar por cliente" value={filtro.clienteId} onChange={(e) => setFiltro({ ...filtro, clienteId: e.target.value ? Number(e.target.value) : '' })}>
            <option value="">Todos os clientes</option>
            {clientes.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
          </select>
        </div>

        <div className="tabela-wrapper">
          <table className="tabela-responsiva">
            <thead>
              <tr><th>#</th><th>Chamado</th><th>Cliente / Projeto</th><th>Prioridade</th><th>Responsável</th><th>Status</th><th></th></tr>
            </thead>
            <tbody>
              {chamados.map((c) => (
                <tr key={c.id}>
                  <td data-label="#" className="celula-id">{c.id}</td>
                  <td data-label="Chamado" className="celula-titulo">
                    <strong><span className="id-cartao">#{c.id} </span>{c.titulo}</strong>
                    <div className="muted pequeno">Aberto em {formatarData(c.criadoEm)}</div>
                  </td>
                  <td data-label="Cliente">
                    <div>{c.clienteNome}<div className="muted pequeno">{c.projetoNome ?? 'Sem projeto'}</div></div>
                  </td>
                  <td data-label="Prioridade"><span className={`badge prioridade-${c.prioridade}`}>{rotulo(c.prioridade)}</span></td>
                  <td data-label="Responsável">{c.responsavelNome ?? <span className="muted">—</span>}</td>
                  <td data-label="Status">
                    <select
                      aria-label={`Status do chamado ${c.id}`}
                      value={c.status}
                      disabled={c.status === 'Cancelado'}
                      onChange={(e) => alterarStatus(c, e.target.value as StatusChamado)}
                    >
                      {STATUS_CHAMADO.map((s) => <option key={s} value={s}>{rotulo(s)}</option>)}
                    </select>
                  </td>
                  <td className="acoes-linha">
                    {c.status !== 'Cancelado' && <button className="btn-link" onClick={() => editar(c)}>Editar</button>}
                    <button className="btn-link" onClick={() => verHistorico(c)}>Histórico</button>
                  </td>
                </tr>
              ))}
              {chamados.length === 0 && <tr><td colSpan={7} className="muted vazio">Nenhum chamado encontrado.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>

      {historico && (
        <div className="card">
          <div className="cabecalho-card">
            <h3>Histórico do chamado #{historico.chamado.id} — {historico.chamado.titulo}</h3>
            <button className="btn-link" onClick={() => setHistorico(null)}>Fechar</button>
          </div>
          <ul className="linha-tempo">
            {historico.itens.map((h) => (
              <li key={h.id}>
                <strong>{rotulo(h.acao)}</strong> <span className="muted">• {formatarData(h.dataHora)} • {h.usuarioNome ?? 'sistema'}</span>
                {h.detalhes && <div>{h.detalhes}</div>}
              </li>
            ))}
          </ul>
        </div>
      )}
    </>
  );
}
