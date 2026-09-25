import { createContext, useCallback, useContext, useMemo, useRef, useState, type ReactNode } from 'react';

// Sistema de notificações "toast": mensagens que aparecem por cima da tela e somem sozinhas.
// Uso em qualquer componente:
//   const toast = useToast();
//   toast.sucesso('Cliente criado.');
//   toast.erro('Já existe um cliente com esse documento.');

type TipoToast = 'sucesso' | 'erro' | 'info';

interface ToastItem {
  id: number;
  tipo: TipoToast;
  mensagem: string;
}

interface ToastContextValue {
  sucesso: (mensagem: string) => void;
  erro: (mensagem: string) => void;
  info: (mensagem: string) => void;
}

const ToastContext = createContext<ToastContextValue | undefined>(undefined);

const DURACAO_MS: Record<TipoToast, number> = { sucesso: 4000, info: 4000, erro: 6000 };
const MAXIMO_VISIVEIS = 4;
const ICONES: Record<TipoToast, string> = { sucesso: '✓', erro: '!', info: 'i' };
const TITULOS: Record<TipoToast, string> = { sucesso: 'Sucesso', erro: 'Erro', info: 'Aviso' };

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const proximoId = useRef(1);

  const remover = useCallback((id: number) => {
    setToasts((lista) => lista.filter((t) => t.id !== id));
  }, []);

  const mostrar = useCallback(
    (tipo: TipoToast, mensagem: string) => {
      const id = proximoId.current++;
      // Mantém no máximo 4 toasts na tela: os mais antigos saem primeiro.
      setToasts((lista) => [...lista.slice(-(MAXIMO_VISIVEIS - 1)), { id, tipo, mensagem }]);
      window.setTimeout(() => remover(id), DURACAO_MS[tipo]);
    },
    [remover],
  );

  // useMemo: o objeto é o mesmo entre renderizações, então pode ser usado em useEffect sem loop.
  const valor = useMemo<ToastContextValue>(
    () => ({
      sucesso: (m) => mostrar('sucesso', m),
      erro: (m) => mostrar('erro', m),
      info: (m) => mostrar('info', m),
    }),
    [mostrar],
  );

  return (
    <ToastContext.Provider value={valor}>
      {children}
      <div className="toasts">
        {toasts.map((t) => (
          <div key={t.id} className={`toast toast-${t.tipo}`} role={t.tipo === 'erro' ? 'alert' : 'status'}>
            <span className="toast-icone" aria-hidden="true">{ICONES[t.tipo]}</span>
            <div className="toast-texto">
              <strong>{TITULOS[t.tipo]}</strong>
              <span>{t.mensagem}</span>
            </div>
            <button className="toast-fechar" aria-label="Fechar notificação" onClick={() => remover(t.id)}>
              ×
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast precisa estar dentro de <ToastProvider>');
  return ctx;
}
