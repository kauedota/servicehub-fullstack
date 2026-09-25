import { useEffect, useState, type FormEvent } from 'react';
import { clientesApi, projetosApi, usuariosApi } from '../api/services';
import { STATUS_PROJETO, type Cliente, type Projeto, type StatusProjeto, type Usuario } from '../api/types';
import { useToast } from '../components/Toast';
import { rotulo } from '../utils/format';

export default function ProjetosPage() {
  const toast = useToast();
  const [projetos, setProjetos] = useState<Projeto[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [filtroCliente, setFiltroCliente] = useState<number | ''>('');
  const [form, setForm] = useState({ clienteId: '', nome: '', descricao: '', responsavelId: '' });

  useEffect(() => {
    // Carrega os dados auxiliares dos selects em paralelo
    Promise.all([clientesApi.listar(), usuariosApi.listar()])
      .then(([c, u]) => { setClientes(c); setUsuarios(u); })
      .catch((e: Error) => toast.erro(e.message));
  }, [toast]);

  useEffect(() => {
    projetosApi.listar(filtroCliente || undefined).then(setProjetos).catch((e: Error) => toast.erro(e.message));
  }, [filtroCliente, toast]);

  async function recarregar() {
    setProjetos(await projetosApi.listar(filtroCliente || undefined));
  }

  async function criar(e: FormEvent) {
    e.preventDefault();
    try {
      const projeto = await projetosApi.criar({
        clienteId: Number(form.clienteId),
        nome: form.nome,
        descricao: form.descricao,
        responsavelId: Number(form.responsavelId),
      });
      toast.sucesso(`Projeto "${projeto.nome}" criado.`);
      setForm({ clienteId: '', nome: '', descricao: '', responsavelId: '' });
      await recarregar();
    } catch (e) {
      toast.erro((e as Error).message);
    }
  }

  async function alterarStatus(p: Projeto, status: StatusProjeto) {
    try {
      const atualizado = await projetosApi.alterarStatus(p.id, status);   // PATCH /api/projetos/{id}/status
      setProjetos((lista) => lista.map((x) => (x.id === p.id ? atualizado : x)));
      toast.sucesso(`"${p.nome}" agora está ${rotulo(status)}.`);
    } catch (e) {
      toast.erro((e as Error).message);
    }
  }

  return (
    <>
      <h2>Projetos</h2>

      <form className="card form-grid" onSubmit={criar}>
        <h3 className="span-todo">Novo projeto</h3>
        <label>
          Cliente
          <select value={form.clienteId} onChange={(e) => setForm({ ...form, clienteId: e.target.value })} required>
            <option value="">Selecione...</option>
            {clientes.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
          </select>
        </label>
        <label>Nome<input value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required /></label>
        <label>
          Responsável
          <select value={form.responsavelId} onChange={(e) => setForm({ ...form, responsavelId: e.target.value })} required>
            <option value="">Selecione...</option>
            {usuarios.map((u) => <option key={u.id} value={u.id}>{u.nome}</option>)}
          </select>
        </label>
        <label className="span-todo">
          Descrição
          <textarea rows={2} value={form.descricao} onChange={(e) => setForm({ ...form, descricao: e.target.value })} />
        </label>
        <div className="acoes span-todo"><button className="btn">Cadastrar projeto</button></div>
      </form>

      <div className="card">
        <div className="barra-busca">
          <select value={filtroCliente} onChange={(e) => setFiltroCliente(e.target.value ? Number(e.target.value) : '')}>
            <option value="">Todos os clientes</option>
            {clientes.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
          </select>
        </div>
        <div className="tabela-wrapper">
          <table className="tabela-responsiva">
            <thead>
              <tr><th>Projeto</th><th>Cliente</th><th>Responsável</th><th>Status</th></tr>
            </thead>
            <tbody>
              {projetos.map((p) => (
                <tr key={p.id}>
                  <td data-label="Projeto" className="celula-titulo">
                    <strong>{p.nome}</strong>
                    {p.descricao && <div className="muted pequeno">{p.descricao}</div>}
                  </td>
                  <td data-label="Cliente">{p.clienteNome}</td>
                  <td data-label="Responsável">{p.responsavelNome}</td>
                  <td data-label="Status">
                    <select
                      aria-label={`Status do projeto ${p.nome}`}
                      value={p.status}
                      onChange={(e) => alterarStatus(p, e.target.value as StatusProjeto)}
                    >
                      {STATUS_PROJETO.map((s) => <option key={s} value={s}>{rotulo(s)}</option>)}
                    </select>
                  </td>
                </tr>
              ))}
              {projetos.length === 0 && <tr><td colSpan={4} className="muted vazio">Nenhum projeto.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}
