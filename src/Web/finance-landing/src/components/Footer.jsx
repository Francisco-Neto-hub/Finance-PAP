import React from 'react';

const Footer = () => {
  return (
    <footer className="main-footer">
      <div className="footer-content">
        <p>&copy; {new Date().getFullYear()} Finance App Project - Gestão Inteligente Local</p>
        <div className="footer-credits">
          <small>Desenvolvido para PAP (Prova de Aptidão Profissional)</small>
        </div>
      </div>
    </footer>
  );
};

export default Footer;