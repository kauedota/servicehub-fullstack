const ROTULOS: Record<string, string> = {
  EmAndamento: 'Em andamento',
  Concluido: 'Concluído',
  Media: 'Média',
  Critica: 'Crítica',
  AlteracaoStatus: 'Alteração de status',
  Criacao: 'Criação',
  Atualizacao: 'Atualização',
  Exclusao: 'Exclusão',
};

/** Converte o valor técnico do enum ("EmAndamento") em texto para a tela ("Em andamento"). */
export const rotulo = (valor: string) => ROTULOS[valor] ?? valor;

/** A API devolve datas em UTC (ISO com "Z"); aqui exibimos no fuso do navegador. */
export const formatarData = (iso: string | null) =>
  iso ? new Date(iso).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' }) : '—';
