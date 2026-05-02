import React from 'react';
import { Settings, ShieldCheck, Zap, Download } from 'lucide-react';

const Manual = () => {
  const steps = [
    {
      icon: <Settings size={32} className="text-blue-500" />,
      title: "1. Instalação",
      description: "Após o download, execute o instalador. O Finance utiliza o nosso Protocolo de Isolamento Local, o que significa que ele não precisa de internet para funcionar após instalado."
    },
    {
      icon: <ShieldCheck size={32} className="text-blue-500" />,
      title: "2. Configuração do 'Bunker'",
      description: "Ao iniciar, defina a sua chave mestra. Lembre-se: os seus dados não vão para a nuvem, ficam guardados apenas no seu disco rígido de forma encriptada."
    },
    {
      icon: <Zap size={32} className="text-blue-500" />,
      title: "3. Radar de Gastos",
      description: "Para detetar aquelas assinaturas esquecidas, importe o seu extrato e a aplicação irá destacar automaticamente pagamentos recorrentes que podem ser 'gastos fantasmas'."
    }
  ];

  return (
    <section id="manual" className="container">
      <h2>Guia de Utilização</h2>
      <div className="manual-grid">
        {steps.map((step, index) => (
          <div key={index} className="credit-card">
            <div className="manual-icon-header">
              {step.icon}
              <h3>{step.title}</h3>
            </div>
            <p>{step.description}</p>
          </div>
        ))}
      </div>
      
      <div style={{ marginTop: '30px', textAlign: 'center' }}>
        {/* Transformamos o button num link <a> com o atributo download */}
        <a 
          href="/Manual_Utilizador_Finance.pdf" 
          download="Manual_Finance.pdf"
          className="btn-download" 
          style={{ 
            padding: '10px 20px', 
            fontSize: '0.9rem', 
            textDecoration: 'none', 
            display: 'inline-flex' 
          }}
        >
          <Download size={18} style={{ marginRight: '10px' }} />
          Descarregar Manual Completo (PDF)
        </a>
      </div>
    </section>
  );
};

export default Manual;