import { createContext, useContext, useState, type ReactNode } from 'react';
import { authApi } from '../api/services';
import { TOKEN_KEY, USUARIO_KEY } from '../api/http';
import type { Usuario } from '../api/types';

// Contexto de autenticação: guarda o usuário logado e expõe login/logout para toda a aplicação.
// O JWT fica no localStorage e é enviado pelo http.ts em cada requisição.
// (Em produção, uma alternativa mais segura contra XSS é cookie HttpOnly.)

interface AuthContextValue {
  usuario: Usuario | null;
  isAdmin: boolean;
  login: (email: string, senha: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function lerUsuarioSalvo(): Usuario | null {
  try {
    const bruto = localStorage.getItem(USUARIO_KEY);
    return bruto && localStorage.getItem(TOKEN_KEY) ? (JSON.parse(bruto) as Usuario) : null;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(lerUsuarioSalvo);

  async function login(email: string, senha: string) {
    const resposta = await authApi.login(email, senha);
    localStorage.setItem(TOKEN_KEY, resposta.token);
    localStorage.setItem(USUARIO_KEY, JSON.stringify(resposta.usuario));
    setUsuario(resposta.usuario);
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USUARIO_KEY);
    setUsuario(null);
  }

  return (
    <AuthContext.Provider value={{ usuario, isAdmin: usuario?.perfil === 'Admin', login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth precisa estar dentro de <AuthProvider>');
  return ctx;
}
