import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import LandingPage from './pages/LandingPage';
import Login from './pages/Login';
import Dashboard from './pages/admin/Dashboard';
import PrivateRoute from './components/PrivateRoute';
import AdminLayout from './pages/admin/AdminLayout';
import Clientes from './pages/admin/Clientes'
import Tickets from './pages/admin/Tickets';
import Categorias from './pages/admin/Categorias';
import Contas from './pages/admin/Contas';
import AuditoriaTransacoes from './pages/admin/AuditoriaTransacoes';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<Login />} />

        {/* Área Protegida */}
        <Route path="/admin" element={<PrivateRoute><AdminLayout /></PrivateRoute>}>
          <Route index element={<Dashboard />} />
          <Route path="clientes" element={<Clientes />} />
          <Route path="contas" element={<Contas />} />
          <Route path="categorias" element={<Categorias />} />
          <Route path="tickets" element={<Tickets />} />
          <Route path="auditoria" element={<AuditoriaTransacoes />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;