import React from 'react';
import { Mail, Github, Linkedin, Send } from 'lucide-react';

const Contactos = () => {
  return (
    <section id="contactos" className="container">
      <h2>Contactos & Suporte</h2>
      
      <div className="contact-grid">
        {/* Formulário Estático */}
        <div className="contact-form">
          <h3>Envia-nos uma Mensagem</h3>
          <form onSubmit={(e) => e.preventDefault()}>
            <div className="form-group">
                <label>Nome</label>
                <input type="text" placeholder="Teu nome" />
            </div>
            <div className="form-group">
                <label>Email</label>
                <input type="email" placeholder="teu@email.com" />
            </div>
            <div className="form-group">
                <label>Mensagem</label>
                <textarea placeholder="Como podemos ajudar?"></textarea>
            </div>
            <button type="submit" className="btn-download" style={{ width: '100%', justifyContent: 'center' }}>
              <Send size={18} style={{ marginRight: '10px' }} />
              Enviar Mensagem
            </button>
          </form>
        </div>

        {/* Links Sociais */}
        <div className="contact-info">
          <h3>Canais Oficiais</h3>
          <p>Tens dúvidas sobre o Protocolo de Isolamento Local? Fala connosco.</p>
          
          <div className="social-links">
            <a href="mailto:suporte@finance.pt" className="social-item">
              <Mail color="#00C2FF" />
              <span>suporte@finance.pt</span>
            </a>
            <a href="https://github.com" target="_blank" className="social-item">
              <Github color="#00C2FF" />
              <span>GitHub do Projeto</span>
            </a>
            <a href="https://linkedin.com" target="_blank" className="social-item">
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