import React from 'react';
import { Link } from 'react-router-dom';

const Footer = () => {
  return (
    <footer className="main-footer">
      <div className="footer-content">
        <p>&copy; {new Date().getFullYear()} Finance App Project - Gestão Inteligente Local</p>
        <div className="footer-credits">
          <small>Desenvolvido para PAP (Prova de Aptidão Profissional)</small>   

          {/* O link "secreto" para o login do admin */}
                <Link to="/login" className="admin-access-link">
                    Acesso Restrito &bull; Administração
                </Link>      
        </div>
      </div>
    </footer>
  );
};

export default Footer;