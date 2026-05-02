import React, { useState } from 'react';
import logo from '../assets/logo_Financepgn.png';
import { Menu, X } from 'lucide-react'; // Importamos os ícones de Menu e Fechar

const Navbar = () => {
  const [isOpen, setIsOpen] = useState(false);

  // Função para inverter o estado do menu
  const toggleMenu = () => {
    setIsOpen(!isOpen);
  };

  // Função para fechar o menu ao clicar num link
  const closeMenu = () => {
    setIsOpen(false);
  };

  return (
    <nav className="navbar">
      <div className="logo-container">
        <img src={logo} alt="Finance Logo" className="nav-logo" />
        <span className="logo-text">FINANCE</span>
      </div>

      {/* Ícone do Menu Hambúrguer (Só visível em Mobile) */}
      <div className="menu-icon" onClick={toggleMenu}>
        {isOpen ? <X size={28} /> : <Menu size={28} />}
      </div>

      {/* Links de Navegação - Adicionamos a classe 'active' se o menu estiver aberto */}
      <ul className={`nav-links ${isOpen ? 'active' : ''}`}>
        <li><a href="#home" onClick={closeMenu}>Home</a></li>
        <li><a href="#features" onClick={closeMenu}>Vantagens</a></li>
        <li><a href="#manual" onClick={closeMenu}>Manual</a></li>
        <li><a href="#contactos" onClick={closeMenu}>Contactos</a></li>
        <li><a href="#creditos" onClick={closeMenu}>Créditos</a></li>
      </ul>
    </nav>
  );
};

export default Navbar;