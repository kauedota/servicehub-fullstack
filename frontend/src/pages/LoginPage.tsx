import { useState, type FormEvent } from 'react';
import { useAuth } from '../auth/AuthContext';
import { useToast } from '../components/Toast';

export default function LoginPage() {
  const { login } = useAuth();
  const toast = useToast();
  const [email, setEmail] = useState('admin@servicehub.local');
  const [senha, setSenha] = useState('Admin@123');
  const [carregando, setCarregando] = useState(false);

  async function entrar(e: FormEvent) {
    e.preventDefault();
    setCarregando(true);
    try {
      await login(email, senha);   // POST /api/auth/login -> recebe o JWT
    } catch (err) {
      toast.erro((err as Error).message);
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="login-pagina">
      <form className="card login-card" onSubmit={entrar}>
        <h1>Service Hub</h1>
        <p className="muted">Gestão de clientes, projetos e chamados</p>

        <label>
          E-mail
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label>
          Senha
          <input type="password" value={senha} onChange={(e) => setSenha(e.target.value)} required />
        </label>

        <button className="btn" disabled={carregando}>{carregando ? 'Entrando...' : 'Entrar'}</button>

        <p className="dica">
          Admin: admin@servicehub.local / Admin@123<br />
          Analista: ana@servicehub.local / Analista@123
        </p>
      </form>
    </div>
  );
}
