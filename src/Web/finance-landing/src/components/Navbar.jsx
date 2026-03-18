import React from 'react';
import logo from '../assets/logo_Financepgn.png';

const Navbar = () => {
  return (
    <nav className="navbar">
      <div className="logo-container">
        <img src={logo} alt="Finance Logo" className="nav-logo" />
        <span className="logo-text">FINANCE</span>
      </div>
      <ul className="nav-links">
        <li><a href="#home">Home</a></li>
        <li><a href="#manual">Manual</a></li>
        <li><a href="#creditos">Créditos</a></li>
      </ul>
    </nav>
  );
};

export default Navbar;