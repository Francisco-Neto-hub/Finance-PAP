import React from 'react';
import { User, Shield, Star } from 'lucide-react';

const Creditos = () => {
  return (
    <section id="creditos" className="container">
      <h2>Equipa de Desenvolvimento</h2>
      
      <div className="credit-card">
        <div className="manual-icon-header">
          <User size={24} color="#00C2FF" />
          <strong>João Silva</strong>
        </div>
        <p style={{ marginLeft: '40px' }}>Lead Developer & UI Design. Responsável pela arquitetura do software e interface principal.</p>
      </div>

      <div className="credit-card">
        <div className="manual-icon-header">
          <Shield size={24} color="#00C2FF" />
          <strong>Maria Santos</strong>
        </div>
        <p style={{ marginLeft: '40px' }}>Backend & Segurança. Especialista em criptografia e gestão de bases de dados locais.</p>
      </div>

      <div className="credit-card" style={{ borderLeftColor: '#f1c40f' }}>
        <div className="manual-icon-header">
          <Star size={24} color="#f1c40f" />
          <h3>Agradecimentos</h3>
        </div>
        <p style={{ marginLeft: '40px' }}>Agradecemos a todos os beta-testers que ajudaram a tornar o Finance uma ferramenta mais robusta.</p>
      </div>
    </section>
  );
};

export default Creditos;