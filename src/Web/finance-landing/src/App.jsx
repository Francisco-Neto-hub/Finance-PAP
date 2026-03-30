import React, { useEffect } from 'react'; // Atualiza o import
import './styles/App.css';
import Navbar from './components/Navbar';
import Footer from './components/Footer';
import Manual from './components/Manual';
import Contactos from './components/Contactos';
import Creditos from './components/Creditos';
import Features from './components/Features'; // Importa aqui
import { Download, Monitor } from 'lucide-react';

function App() {
  useEffect(() => {
    const observerOptions = { threshold: 0.1 };
    
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('reveal-visible');
        }
      });
    }, observerOptions);

    // Seleciona todas as secções e cards para animar
    const elements = document.querySelectorAll('section, .feature-card, .credit-card');
    elements.forEach(el => {
      el.classList.add('reveal-hidden');
      observer.observe(el);
    });

    // Função de limpeza (cleanup)
    return () => observer.disconnect();
  }, []);

  return (
    <div className="app-wrapper">
      <Navbar />

      <main className="main-content">
        {/* O Hero foca apenas na chamada para ação e download */}
        <header id="home" className="hero">
          <div className="hero-content">
            <div className="mockup-container">
              <div className="app-icon-wrapper">
                <Monitor size={80} color="#00C2FF" />
                <div className="download-badge">.exe</div>
              </div>
            </div>
            <h1 className="hero-title">Domine as suas Finanças com Privacidade Total</h1>
            <p className="hero-subtitle">
              A ferramenta desktop definitiva para quem procura liberdade financeira sem guardar dados na nuvem.
            </p>
            <a href="#" className="btn-download">
              <Download size={20} style={{ marginRight: "10px" }} />
              DESCARREGAR FINANCE V1.0
            </a>
          </div>
        </header>

        {/* As Features aparecem logo após o Hero para explicar o valor do produto */}
        <Features />

        <Manual />
        <Contactos />
        <Creditos />
      </main>
      <Footer />
    </div>
  );
}

export default App;