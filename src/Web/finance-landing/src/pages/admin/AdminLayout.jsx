import React from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { LayoutDashboard, Users, Landmark, ShieldAlert, Tags, MessageSquare, LogOut } from 'lucide-react';
import '../../styles/Admin.css';

const AdminLayout = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  return (
    <div className="admin-container">
      {/* SIDEBAR */}
      <aside className="sidebar">
        <div className="sidebar-header">
          FINANCE ADMIN
        </div>
        
        <nav className="sidebar-nav">
          <NavLink to="/admin" end className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
            <LayoutDashboard size={20} /> Painel Geral
          </NavLink>
          
          <NavLink to="/admin/clientes" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
            <Users size={20} /> Clientes & Acessos
          </NavLink>

          {/* NOVOS LINKS */}
          <NavLink to="/admin/contas" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
            <Landmark size={20} /> Contas Bancárias
          </NavLink>

          <NavLink to="/admin/auditoria" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
            <ShieldAlert size={20} /> Auditoria Global
          </NavLink>

          <NavLink to="/admin/categorias" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
            <Tags size={20} /> Categorias
          </NavLink>

          <NavLink to="/admin/tickets" className={({ isActive }) => isActive ? "nav-item active" : "nav-item"}>
            <MessageSquare size={20} /> Centro de Suporte
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          <button onClick={handleLogout} className="nav-item" style={{ width: '100%', background: 'transparent', border: 'none', cursor: 'pointer', textAlign: 'left', color: '#EF4444' }}>
            <LogOut size={20} /> Encerrar Sessão
          </button>
        </div>
      </aside>

      {/* ÁREA DE CONTEÚDO (Onde as páginas são renderizadas) */}
      <main className="admin-content">
        <Outlet />
      </main>
    </div>
  );
};

export default AdminLayout;