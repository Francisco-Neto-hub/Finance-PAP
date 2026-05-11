import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import logo from '../assets/logo_Financepgn.png';
import { Loader2, AlertCircle } from 'lucide-react';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      // 1. O envio deve respeitar o DTO do C# (Geralmente PascalCase: Email, Password)
      // Se o teu DTO usa letras minúsculas, podes manter, mas Email/Password é o padrão C#.
      const response = await axios.post('https://localhost:7221/api/Auth/login', {
        Email: email,    // Mudei para 'E' maiúsculo
        Password: password // Mudei para 'P' maiúsculo
      });

      // 2. IMPORTANTE: O teu backend devolve "token" e "perfil" (e não "role")
      const { token, perfil } = response.data;

      // 3. Verificação do perfil
      if (perfil !== 'Admin') {
        setError('Acesso negado. Esta área é restrita a administradores.');
        setLoading(false);
        return;
      }

      // 4. Guardar no localStorage
      // Guardamos como 'role' para o teu PrivateRoute continuar a funcionar sem alterações
      localStorage.setItem('token', token);
      localStorage.setItem('role', perfil); 

      // 5. Navegar para o Admin
      navigate('/admin');

    } catch (err) {
      // Se a API devolver 401, cai aqui
      console.error("Erro detalhado:", err.response?.data);
      setError(err.response?.data?.mensagem || 'Credenciais inválidas ou erro de ligação.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-card">
        <img src={logo} alt="Finance Logo" width="80" className="login-logo" />
        <h2>Controlo de Acesso</h2>
        <p className="login-subtitle">Área Administrativa Finance</p>

        {error && (
          <div className="login-error">
            <AlertCircle size={18} /> {error}
          </div>
        )}

        <form onSubmit={handleLogin}>
          <div className="input-group">
            <label>Utilizador (Email)</label>
            <input 
              type="email" 
              placeholder="exemplo@finance.pt" 
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required 
            />
          </div>

          <div className="input-group">
            <label>Palavra-passe</label>
            <input 
              type="password" 
              placeholder="••••••••" 
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required 
            />
          </div>

          <button type="submit" className="btn-download" disabled={loading} style={{ width: '100%' }}>
            {loading ? <Loader2 className="spin-animation" /> : "Entrar no Sistema"}
          </button>
        </form>
      </div>
    </div>
  );
};

export default Login;