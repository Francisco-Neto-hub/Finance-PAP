import React from 'react';
import { ShieldCheck, PieChart, HardDrive, RefreshCw } from 'lucide-react';

const Features = () => {
  const features = [
    {
      icon: <ShieldCheck size={40} color="#00C2FF" />,
      title: "Privacidade Total",
      desc: "Os seus dados nunca saem do seu computador. Sem nuvem, sem riscos."
    },
    {
      icon: <PieChart size={40} color="#00C2FF" />,
      title: "Análise Visual",
      desc: "Gráficos intuitivos para entender exatamente para onde vai o seu dinheiro."
    },
    {
      icon: <HardDrive size={40} color="#00C2FF" />,
      title: "Base de Dados Local",
      desc: "Utiliza SQL Server Express para uma gestão de dados robusta e profissional."
    },
    {
      icon: <RefreshCw size={40} color="#00C2FF" />,
      title: "Controlo de Fluxo",
      desc: "Registe receitas e despesas com validação de saldo em tempo real."
    }
  ];

  return (
    <section id="features" className="container" style={{ background: 'transparent', boxShadow: 'none' }}>
      <div className="features-grid">
        {features.map((f, i) => (
          <div key={i} className="feature-card">
            <div className="feature-icon">{f.icon}</div>
            <h3>{f.title}</h3>
            <p>{f.desc}</p>
          </div>
        ))}
      </div>
    </section>
  );
};

export default Features;