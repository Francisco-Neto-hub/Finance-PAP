import React, { useEffect, useState } from 'react'; // Adicionado useState
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';
import Manual from '../components/Manual';
import Contactos from '../components/Contactos';
import Creditos from '../components/Creditos';
import Features from '../components/Features';
import { Download, Monitor, CheckCircle, Loader2, X } from 'lucide-react'; // Ícones extra

const LandingPage = () => {
  const [isDownloading, setIsDownloading] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [progress, setProgress] = useState(0);

  useEffect(() => {
      // Efeito de scroll (já tinhas)
    const observerOptions = { threshold: 0.1 };
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) entry.target.classList.add('reveal-visible');
      });
    }, observerOptions);

    const elements = document.querySelectorAll('section, .feature-card, .credit-card');
    elements.forEach(el => {
      el.classList.add('reveal-hidden');
      observer.observe(el);
    });
    return () => observer.disconnect();
  }, []);
    
    // Lógica do Download
  const handleDownload = (e) => {
    e.preventDefault();
    setIsDownloading(true);
    setProgress(0);

    // Simulação de progresso de download
    const interval = setInterval(() => {
      setProgress((prev) => {
        if (prev >= 100) {
          clearInterval(interval);
          setIsDownloading(false);
          setShowModal(true);
          // Aqui é onde no futuro colocarias: window.location.href = '/caminho/para/finance.exe';
          return 100;
        }
        return prev + 5;
      });
    }, 100);
  };

  return (
    <div className="app-wrapper">
      <Navbar />

      <main className="main-content">
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
            
            {/* Botão com Feedback Visual */}
            <div className="download-area">
              <button 
                onClick={handleDownload} 
                className={`btn-download ${isDownloading ? 'active' : ''}`}
                disabled={isDownloading}
              >
                {isDownloading ? (
                  <>
                    <Loader2 size={20} className="spin-animation" style={{ marginRight: "10px" }} />
                    A PREPARAR INSTALADOR ({progress}%)
                  </>
                ) : (
                  <>
                    <Download size={20} style={{ marginRight: "10px" }} />
                    DESCARREGAR FINANCE V1.0
                  </>
                )}
              </button>
              
              {isDownloading && (
                <div className="progress-bar-container">
                  <div className="progress-bar-fill" style={{ width: `${progress}%` }}></div>
                </div>
              )}
            </div>
          </div>
        </header>

        <Features />
        <Manual />
        <Contactos />
        <Creditos />
      </main>

      <Footer />

      {/* Modal de Sucesso/Instruções */}
            {showModal && (
              <div className="modal-overlay">
                <div className="modal-content">
                  <button className="close-modal" onClick={() => setShowModal(false)}><X size={20}/></button>
                  <CheckCircle size={50} color="#10B981" />
                  <h2>Download Iniciado!</h2>
                  <p>O instalador do <strong>Finance V1.0</strong> está a ser transferido.</p>
                  <div className="setup-steps">
                    <h4>Próximos Passos:</h4>
                    <ol>
                      <li>Executa o ficheiro <code>Finance_Setup.exe</code>.</li>
                      <li>Garante que tens o <strong>SQL Server Express</strong> instalado.</li>
                      <li>Cria a tua Chave Mestra para encriptação local.</li>
                    </ol>
                  </div>
                  <button className="btn-download" onClick={() => setShowModal(false)}>Entendido</button>
                </div>
              </div>
            )}
          </div>
  );
};

export default LandingPage;