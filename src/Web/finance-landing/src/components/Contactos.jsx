import React, { useState } from 'react';
import { Mail, Github, Linkedin, Send, CheckCircle, AlertCircle, Loader2 } from 'lucide-react';

const Contactos = () => {
  // 1. Estado para guardar os dados do formulário
  const [formData, setFormData] = useState({
    nome: '',
    email: '',
    mensagem: ''
  });

  // 2. Estado para gerir o feedback visual (loading, sucesso, erro)
  const [status, setStatus] = useState({
    loading: false,
    type: '', // 'success' ou 'error'
    message: ''
  });

  // 3. Função que atualiza o estado quando o utilizador escreve
  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  // 4. Função que lida com o envio do formulário (Agora com Formspree)
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    setStatus({ loading: true, type: '', message: '' });

    try {
      // SUBSTITUI O LINK ABAIXO PELO LINK QUE O FORMSPREE TE DEU
      const response = await fetch('https://formspree.io/f/xdabkble', {
        method: 'POST',
        headers: {
          'Accept': 'application/json', // Obrigatório para o Formspree
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });

      if (response.ok) {
        setStatus({ loading: false, type: 'success', message: 'Mensagem enviada com sucesso! Verifica o teu email.' });
        setFormData({ nome: '', email: '', mensagem: '' }); // Limpa o formulário
      } else {
        setStatus({ loading: false, type: 'error', message: 'Ocorreu um erro ao enviar a mensagem.' });
      }
    } catch (error) {
      setStatus({ loading: false, type: 'error', message: 'Erro de ligação. Verifica a tua internet.' });
    }
    
    // Limpa a mensagem de feedback após 5 segundos
    setTimeout(() => setStatus({ loading: false, type: '', message: '' }), 5000);
  };

  return (
    <section id="contactos" className="container">
      <h2>Contactos & Suporte</h2>
      
      <div className="contact-grid">
        {/* Formulário Dinâmico */}
        <div className="contact-form">
          <h3>Envia-nos uma Mensagem</h3>
          <form onSubmit={handleSubmit}>
            <div className="form-group">
                <label>Nome</label>
                <input 
                  type="text" 
                  name="nome"
                  placeholder="Teu nome" 
                  value={formData.nome}
                  onChange={handleChange}
                  required 
                />
            </div>
            <div className="form-group">
                <label>Email</label>
                <input 
                  type="email" 
                  name="email"
                  placeholder="teu@email.com" 
                  value={formData.email}
                  onChange={handleChange}
                  required 
                />
            </div>
            <div className="form-group">
                <label>Mensagem</label>
                <textarea 
                  name="mensagem"
                  placeholder="Como podemos ajudar?" 
                  value={formData.mensagem}
                  onChange={handleChange}
                  required 
                />
            </div>

            {/* Mensagens de Feedback */}
            {status.message && (
              <div style={{
                padding: '10px',
                marginBottom: '15px',
                borderRadius: '5px',
                display: 'flex',
                alignItems: 'center',
                gap: '8px',
                backgroundColor: status.type === 'success' ? '#10B98120' : '#EF444420',
                color: status.type === 'success' ? '#10B981' : '#EF4444',
                border: `1px solid ${status.type === 'success' ? '#10B981' : '#EF4444'}`
              }}>
                {status.type === 'success' ? <CheckCircle size={18} /> : <AlertCircle size={18} />}
                {status.message}
              </div>
            )}

            <button 
              type="submit" 
              className="btn-download" 
              disabled={status.loading}
              style={{ width: '100%', justifyContent: 'center', opacity: status.loading ? 0.7 : 1 }}
            >
              {status.loading ? (
                <Loader2 size={18} className="spin-animation" style={{ marginRight: '10px' }} />
              ) : (
                <Send size={18} style={{ marginRight: '10px' }} />
              )}
              {status.loading ? 'A enviar...' : 'Enviar Mensagem'}
            </button>
          </form>
        </div>

        {/* Links Sociais */}
        <div className="contact-info">
          <h3>Canais Oficiais</h3>
          <p>Tens dúvidas sobre o Protocolo de Isolamento Local? Fala connosco.</p>
          
          <div className="social-links">
            <a href="mailto:a115943@aeg1.pt" className="social-item">
              <Mail color="#00C2FF" />
              <span>suporte@finance.pt</span>
            </a>
            <a href="https://github.com/Francisco-Neto-hub/Finance-PAP" target="_blank" rel="noreferrer" className="social-item">
              <Github color="#00C2FF" />
              <span>GitHub do Projeto</span>
            </a>
            <a href="https://linkedin.com" target="_blank" rel="noreferrer" className="social-item">
              <Linkedin color="#00C2FF" />
              <span>LinkedIn</span>
            </a>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Contactos;