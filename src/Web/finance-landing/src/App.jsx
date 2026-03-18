import React from 'react';
import './styles/App.css';
import Navbar from './components/Navbar';
import Footer from './components/Footer';
import Manual from './components/Manual';
import Contactos from './components/Contactos';
import Creditos from './components/Creditos';
import { Download, Monitor } from 'lucide-react';

function App() {
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
            <a href="#" className="btn-download">
              <Download size={20} style={{ marginRight: '10px' }} />
              DESCARREGAR FINANCE V1.0
            </a>
          </div>
        </header>

        {/* Aqui entrarão as próximas secções (Manual, etc.) */}
        <Manual />
        <Contactos />
        <Creditos />

      </main>
      <Footer />
    </div>
  );
}

export default App;