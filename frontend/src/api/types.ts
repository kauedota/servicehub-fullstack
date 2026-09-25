// Tipos TypeScript espelhando os DTOs do backend C#.
// Os enums do C# chegam como TEXTO no JSON (JsonStringEnumConverter), por isso usamos union types.

export type Perfil = 'Admin' | 'Analista';
export type StatusProjeto = 'Planejado' | 'EmAndamento' | 'Pausado' | 'Concluido';
export type StatusChamado = 'Aberto' | 'EmAndamento' | 'Concluido' | 'Cancelado';
export type Prioridade = 'Baixa' | 'Media' | 'Alta' | 'Critica';

export interface Usuario {
  id: number;
  nome: string;
  email: string;
  perfil: Perfil;
}

export interface LoginResponse {
  token: string;
  expiraEm: string;
  usuario: Usuario;
}

export interface Cliente {
  id: number;
  nome: string;
  documento: string;
  email: string;
  telefone: string | null;
  setor: string;
  criadoEm: string;
  atualizadoEm: string | null;
}

export interface ClienteRequest {
  nome: string;
  documento: string;
  email: string;
  telefone: string;
  setor: string;
}

export interface Projeto {
  id: number;
  clienteId: number;
  clienteNome: string;
  nome: string;
  descricao: string | null;
  status: StatusProjeto;
  responsavelId: number;
  responsavelNome: string;
  criadoEm: string;
}

export interface ProjetoRequest {
  clienteId: number;
  nome: string;
  descricao: string;
  responsavelId: number;
}

export interface Chamado {
  id: number;
  titulo: string;
  descricao: string;
  prioridade: Prioridade;
  status: StatusChamado;
  clienteId: number;
  clienteNome: string;
  projetoId: number | null;
  projetoNome: string | null;
  responsavelId: number | null;
  responsavelNome: string | null;
  criadoEm: string;
  atualizadoEm: string | null;
  concluidoEm: string | null;
}

export interface ChamadoFiltro {
  status?: StatusChamado | '';
  prioridade?: Prioridade | '';
  clienteId?: number | '';
}

export interface ChamadoRequest {
  titulo: string;
  descricao: string;
  clienteId: number;
  projetoId: number | null;
  responsavelId: number | null;
  prioridade: Prioridade;
}

export interface Auditoria {
  id: number;
  entidade: string;
  entidadeId: number;
  acao: string;
  detalhes: string | null;
  usuarioNome: string | null;
  dataHora: string;
}

export interface Dashboard {
  totalClientes: number;
  totalProjetos: number;
  chamadosAbertos: number;
  chamadosEmAndamento: number;
  chamadosConcluidos: number;
  chamadosPorPrioridade: { prioridade: Prioridade; total: number; emAberto: number }[];
}

export const STATUS_CHAMADO: StatusChamado[] = ['Aberto', 'EmAndamento', 'Concluido', 'Cancelado'];
export const STATUS_PROJETO: StatusProjeto[] = ['Planejado', 'EmAndamento', 'Pausado', 'Concluido'];
export const PRIORIDADES: Prioridade[] = ['Baixa', 'Media', 'Alta', 'Critica'];
export const SETORES = ['Varejo', 'Saúde', 'Finanças', 'Indústria', 'Outros'];
