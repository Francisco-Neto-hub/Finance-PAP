import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Users, Activity, Wallet, ShieldAlert, ArrowUpRight } from 'lucide-react';
import GraficoTransacoes from '../../components/admin/GraficoTransacoes';

const Dashboard = () => {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const token = localStorage.getItem('token');

        const response = await axios.get('https://localhost:7221/api/Admin/dashboard-stats', {
          headers: { Authorization: `Bearer ${token}` }
        });
        setStats(response.data);
      } catch (error) {
        console.error("Erro ao carregar estatísticas", error);
      } finally {
        setLoading(false);
      }
    };
    fetchStats();
  }, []);

  if (loading) return <div className="loading-state">A carregar sistemas de monitorização...</div>;
  if (!stats) return <div className="error-state">Erro ao ligar ao servidor de dados.</div>;

  return (
    <div className="page-content">
      <div className="content-header">
        <h1>Painel de Monitorização</h1>
        <p>Estado em tempo real do ecossistema Finance.</p>
      </div>

      <div className="stats-grid">
        {/* Cartão: Total Clientes */}
        <div className="stat-card">
          <div className="stat-icon-wrapper blue">
            <Users size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Total Clientes</span>
            <span className="stat-value">{stats.totalClientes}</span>
          </div>
        </div>

        {/* Cartão: Ativos */}
        <div className="stat-card">
          <div className="stat-icon-wrapper green">
            <Activity size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Utilizadores Ativos</span>
            <span className="stat-value">{stats.clientesAtivos}</span>
          </div>
          <div className="stat-percentage">
            {((stats.clientesAtivos / stats.totalClientes) * 100).toFixed(0)}% do total
          </div>
        </div>

        {/* Cartão: Volume */}
        <div className="stat-card">
          <div className="stat-icon-wrapper blue">
            <Wallet size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-label">Volume Transações (Mês)</span>
            <span className="stat-value">
              {stats.volumeTransacoesMes.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })}
            </span>
          </div>
        </div>
      </div>

      {/* AQUI ENTRA O GRÁFICO */}
      <div className="charts-grid" style={{ marginTop: '30px' }}>
        <GraficoTransacoes />
      </div>

      <div className="dashboard-row">
        {/* Widget de Auditoria Rápida */}
        <div className="dashboard-widget">
          <div className="widget-header">
            <h3><ShieldAlert size={18} /> Alertas de Segurança Recentes</h3>
            <button className="btn-text">Ver todos</button>
          </div>
          <div className="widget-content">
             {/* Exemplo de item de log que podes popular depois */}
             <div className="log-item mini">
                <div className="status-dot warning"></div>
                <div className="log-text">Alteração de saldo detetada na Conta #12</div>
                <div className="log-time">Agora mesmo</div>
             </div>
             <div className="log-item mini">
                <div className="status-dot success"></div>
                <div className="log-text">Novo Administrador autenticado: Francisco</div>
                <div className="log-time">Há 5 min</div>
             </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;