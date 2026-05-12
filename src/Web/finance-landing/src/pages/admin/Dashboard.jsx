import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Users, Activity, Wallet, ShieldAlert, ArrowRight, Loader2, TrendingUp } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import GraficoTransacoes from '../../components/admin/GraficoTransacoes';

const Dashboard = () => {
  const [stats, setStats] = useState(null);
  const [recentLogs, setRecentLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const token = localStorage.getItem('token');
        const headers = { Authorization: `Bearer ${token}` };

        // 1. Estatísticas Gerais
        const statsRes = await axios.get('https://localhost:7221/api/Admin/dashboard-stats', { headers });
        setStats(statsRes.data);

        // 2. Últimos Logs de Auditoria (para o widget de segurança)
        const logsRes = await axios.get('https://localhost:7221/api/Auditoria/saldos', { headers });
        setRecentLogs(logsRes.data.slice(0, 4)); // Apenas os 4 mais recentes

      } catch (error) {
        console.error("Erro ao carregar dashboard", error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  if (loading) return (
    <div className="loading-state">
      <Loader2 className="spin-animation" size={40} />
      <p>Sincronizando terminais de dados...</p>
    </div>
  );

  return (
    <div className="page-content">
      <div className="content-header">
        <div>
          <h1>Painel de Monitorização</h1>
          <p>Estado em tempo real do ecossistema Finance.</p>
        </div>
        <div className="system-status">
          <span className="status-indicator online"></span> Sistema Operacional
        </div>
      </div>

      <div className="stats-grid">
        {/* Cartão: Total Clientes */}
        <div className="stat-card">
          <div className="stat-icon-wrapper blue">
            <Users size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Total Clientes</span>
            <span className="stat-value">{stats?.totalClientes || 0}</span>
          </div>
        </div>

        {/* Cartão: Ativos */}
        <div className="stat-card">
          <div className="stat-icon-wrapper green">
            <Activity size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Utilizadores Ativos</span>
            <span className="stat-value">{stats?.clientesAtivos || 0}</span>
          </div>
          <div className="stat-footer text-green">
             <TrendingUp size={14} /> {((stats?.clientesAtivos / stats?.totalClientes) * 100).toFixed(0)}% de taxa de retenção
          </div>
        </div>

        {/* Cartão: Volume Mensal */}
        <div className="stat-card">
          <div className="stat-icon-wrapper gold">
            <Wallet size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Volume Transações (Mês)</span>
            <span className="stat-value">
              {stats?.volumeTransacoesMes?.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' }) || '0,00 €'}
            </span>
          </div>
        </div>
      </div>

      <div className="dashboard-main-grid" style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: '25px', marginTop: '30px' }}>
        
        {/* COLUNA ESQUERDA: GRÁFICO */}
        <div className="chart-section" style={{ background: '#111', padding: '20px', borderRadius: '15px', border: '1px solid #222' }}>
          <div className="widget-header" style={{ marginBottom: '20px' }}>
            <h3>Análise de Fluxo de Capital</h3>
          </div>
          <GraficoTransacoes />
        </div>

        {/* COLUNA DIREITA: WIDGETS RÁPIDOS */}
        <div className="dashboard-side-col" style={{ display: 'flex', flexDirection: 'column', gap: '25px' }}>
          
          <div className="dashboard-widget security-widget">
            <div className="widget-header">
              <h3><ShieldAlert size={18} color="#FFD700" /> Auditoria Recente</h3>
              <button className="btn-text" onClick={() => navigate('/admin/auditoria')}>
                Ver Tudo <ArrowRight size={14} />
              </button>
            </div>
            
            <div className="widget-content">
              {recentLogs.map(log => (
                <div key={log.idLog} className="log-item mini">
                  <div className={`status-dot ${log.saldoNovo > log.saldoAntigo ? 'success' : 'warning'}`}></div>
                  <div className="log-text">
                    <strong>{log.nomeConta}</strong>: Alteração de {((log.saldoNovo - log.saldoAntigo)).toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })}
                  </div>
                  <div className="log-time">{new Date(log.dataAlteracao).toLocaleTimeString('pt-PT', {hour: '2-digit', minute:'2-digit'})}</div>
                </div>
              ))}
              {recentLogs.length === 0 && <p style={{color: '#666', fontSize: '0.9rem'}}>Sem registos recentes.</p>}
            </div>
          </div>

          <div className="dashboard-widget info-card" style={{ background: 'linear-gradient(135deg, #00C2FF 0%, #0045FF 100%)', color: 'white' }}>
            <h4>Dica de Gestão</h4>
            <p style={{ opacity: 0.9, fontSize: '0.9rem', marginTop: '10px' }}>
              Utiliza o menu de Tickets para responder aos pedidos de suporte pendentes. Clientes satisfeitos geram mais volume!
            </p>
          </div>

        </div>
      </div>
    </div>
  );
};

export default Dashboard;