import { http } from './http';
import type {
  Auditoria, Chamado, ChamadoFiltro, ChamadoRequest, Cliente, ClienteRequest, Dashboard,
  LoginResponse, Prioridade, Projeto, ProjetoRequest, StatusChamado, StatusProjeto, Usuario,
} from './types';

// Uma função por endpoint da API. Os componentes React nunca chamam fetch diretamente.

const json = (body: unknown) => JSON.stringify(body);

export const authApi = {
  login: (email: string, senha: string) =>
    http<LoginResponse>('/api/auth/login', { method: 'POST', body: json({ email, senha }) }),
};

export const usuariosApi = {
  listar: () => http<Usuario[]>('/api/usuarios'),
};

export const clientesApi = {
  listar: (busca = '') => http<Cliente[]>(`/api/clientes${busca ? `?busca=${encodeURIComponent(busca)}` : ''}`),
  criar: (dados: ClienteRequest) => http<Cliente>('/api/clientes', { method: 'POST', body: json(dados) }),
  atualizar: (id: number, dados: ClienteRequest) =>
    http<Cliente>(`/api/clientes/${id}`, { method: 'PUT', body: json(dados) }),
  excluir: (id: number) => http<void>(`/api/clientes/${id}`, { method: 'DELETE' }),
};

export const projetosApi = {
  listar: (clienteId?: number) => http<Projeto[]>(`/api/projetos${clienteId ? `?clienteId=${clienteId}` : ''}`),
  criar: (dados: ProjetoRequest) => http<Projeto>('/api/projetos', { method: 'POST', body: json(dados) }),
  alterarStatus: (id: number, status: StatusProjeto) =>
    http<Projeto>(`/api/projetos/${id}/status`, { method: 'PATCH', body: json({ status }) }),
};

export const chamadosApi = {
  listar: (filtro: ChamadoFiltro) => {
    const params = new URLSearchParams();
    Object.entries(filtro).forEach(([chave, valor]) => {
      if (valor !== '' && valor !== undefined) params.set(chave, String(valor));
    });
    const qs = params.toString();
    return http<Chamado[]>(`/api/chamados${qs ? `?${qs}` : ''}`);
  },
  criar: (dados: ChamadoRequest) => http<Chamado>('/api/chamados', { method: 'POST', body: json(dados) }),
  atualizar: (id: number, dados: Omit<ChamadoRequest, 'clienteId'> & { prioridade: Prioridade }) =>
    http<Chamado>(`/api/chamados/${id}`, { method: 'PUT', body: json(dados) }),
  alterarStatus: (id: number, status: StatusChamado, comentario?: string) =>
    http<Chamado>(`/api/chamados/${id}/status`, { method: 'PATCH', body: json({ status, comentario }) }),
  historico: (id: number) => http<Auditoria[]>(`/api/chamados/${id}/historico`),
};

export const dashboardApi = {
  obter: () => http<Dashboard>('/api/dashboard'),
};
