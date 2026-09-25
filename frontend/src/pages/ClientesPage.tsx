import { useEffect, useState, type FormEvent } from 'react';
import { clientesApi } from '../api/services';
import { SETORES, type Cliente, type ClienteRequest } from '../api/types';
import { useAuth } from '../auth/AuthContext';
import { useToast } from '../components/Toast';
import { formatarData } from '../utils/format';

const FORM_VAZIO: ClienteRequest = { nome: '', documento: '', email: '', telefone: '', setor: 'Varejo' };

// CRUD completo de clientes: listar, buscar, criar, editar, excluir.
export default function ClientesPage() {
  const { isAdmin } = useAuth();
  const toast = useToast();
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [busca, setBusca] = useState('');
  const [form, setForm] = useState<ClienteRequest>(FORM_VAZIO);
  const [editandoId, setEditandoId] = useState<number | null>(null);

  async function carregar(termo = busca) {
    try {
      setClientes(await clientesApi.listar(termo));          // GET /api/clientes?busca=
    } catch (e) {
      toast.erro((e as Error).message);
    }
  }

  useEffect(() => {
    carregar('');   // [] = executa uma vez, quando a tela abre
  }, []);

  async function salvar(e: FormEvent) {
    e.preventDefault();
    try {
      if (editandoId) {
        await clientesApi.atualizar(editandoId, form);        // PUT /api/clientes/{id}
        toast.sucesso('Cliente atualizado.');
      } else {
        await clientesApi.criar(form);                       // POST /api/clientes
        toast.sucesso('Cliente criado.');
      }
      cancelar();
      await carregar();
    } catch (e) {
      toast.erro((e as Error).message);                      // ex.: documento duplicado (400)
    }
  }

  function editar(c: Cliente) {
    setEditandoId(c.id);
    setForm({ nome: c.nome, documento: c.documento, email: c.email, telefone: c.telefone ?? '', setor: c.setor });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function cancelar() {
    setEditandoId(null);
    setForm(FORM_VAZIO);
  }

  async function excluir(c: Cliente) {
    if (!confirm(`Excluir o cliente "${c.nome}"?`)) return;
    try {
      await clientesApi.excluir(c.id);                       // DELETE /api/clientes/{id} (só Admin)
      toast.sucesso(`Cliente "${c.nome}" excluído.`);
      await carregar();
    } catch (e) {
      toast.erro((e as Error).message);                      // ex.: cliente com chamados (400)
    }
  }

  const campo = (nome: keyof ClienteRequest) => ({
    value: form[nome],
    onChange: (e: { target: { value: string } }) => setForm({ ...form, [nome]: e.target.value }),
  });

  return (
    <>
      <h2>Clientes</h2>

      <form className="card form-grid" onSubmit={salvar}>
        <h3 className="span-todo">{editandoId ? `Editar cliente #${editandoId}` : 'Novo cliente'}</h3>
        <label>Nome<input {...campo('nome')} required /></label>
        <label>CNPJ / CPF<input {...campo('documento')} placeholder="00.000.000/0000-00" required /></label>
        <label>E-mail<input type="email" {...campo('email')} required /></label>
        <label>Telefone<input type="tel" {...campo('telefone')} /></label>
        <label>
          Setor
          <select {...campo('setor')}>
            {SETORES.map((s) => <option key={s}>{s}</option>)}
          </select>
        </label>
        <div className="acoes span-todo">
          <button className="btn">{editandoId ? 'Salvar alterações' : 'Cadastrar'}</button>
          {editandoId && <button type="button" className="btn btn-secundario" onClick={cancelar}>Cancelar</button>}
        </div>
      </form>

      <div className="card">
        <form className="barra-busca" onSubmit={(e) => { e.preventDefault(); carregar(); }}>
          <input placeholder="Buscar por nome, documento ou e-mail" value={busca} onChange={(e) => setBusca(e.target.value)} />
          <button className="btn btn-secundario">Buscar</button>
        </form>

        {/* data-label: no celular a tabela vira uma lista de cartões e o rótulo aparece antes de cada valor */}
        <div className="tabela-wrapper">
          <table className="tabela-responsiva">
            <thead>
              <tr><th>Nome</th><th>Documento</th><th>E-mail</th><th>Setor</th><th>Criado em</th><th></th></tr>
            </thead>
            <tbody>
              {clientes.map((c) => (
                <tr key={c.id}>
                  <td data-label="Nome" className="celula-titulo">{c.nome}</td>
                  <td data-label="Documento">{c.documento}</td>
                  <td data-label="E-mail">{c.email}</td>
                  <td data-label="Setor">{c.setor}</td>
                  <td data-label="Criado em">{formatarData(c.criadoEm)}</td>
                  <td className="acoes-linha">
                    <button className="btn-link" onClick={() => editar(c)}>Editar</button>
                    {isAdmin && <button className="btn-link perigo" onClick={() => excluir(c)}>Excluir</button>}
                  </td>
                </tr>
              ))}
              {clientes.length === 0 && <tr><td colSpan={6} className="muted vazio">Nenhum cliente encontrado.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}
