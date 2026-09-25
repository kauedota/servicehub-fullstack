// Cliente HTTP único da aplicação (wrapper do fetch).
// Responsabilidades: montar a URL, enviar o JWT no header Authorization,
// converter JSON e transformar respostas de erro (ProblemDetails do .NET) em mensagens legíveis.

const BASE_URL = import.meta.env.VITE_API_URL ?? '';
export const TOKEN_KEY = 'servicehub.token';
export const USUARIO_KEY = 'servicehub.usuario';

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export async function http<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body) headers.set('Content-Type', 'application/json');

  const token = localStorage.getItem(TOKEN_KEY);
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const resposta = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  // Token expirado/inválido: limpa a sessão e volta para o login.
  if (resposta.status === 401 && !path.startsWith('/api/auth/login')) {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USUARIO_KEY);
    window.location.href = '/login';
    throw new ApiError(401, 'Sessão expirada.');
  }

  if (!resposta.ok) {
    let corpo: ProblemDetails | null = null;
    try {
      corpo = (await resposta.json()) as ProblemDetails;
    } catch {
      /* resposta sem JSON */
    }
    const mensagem = corpo?.errors
      ? Object.values(corpo.errors).flat().join(' ')          // erros de validação (400)
      : corpo?.detail ?? corpo?.title ?? (resposta.status === 403 ? 'Sem permissão para esta ação.' : `Erro ${resposta.status}`);
    throw new ApiError(resposta.status, mensagem);
  }

  if (resposta.status === 204) return undefined as T;
  return (await resposta.json()) as T;
}
