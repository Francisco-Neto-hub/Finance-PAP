import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Loader2, Lock, Unlock, Search, Landmark, Wallet, ShieldAlert } from 'lucide-react';

const Contas = () => {
  const [contas, setContas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [busca, setBusca] = useState('');

  const fetchContas = async () => {
    try {
      const token = localStorage.getItem('token');
      const res = await axios.get('https://localhost:7221/api/Admin/contas', {
        headers: { Authorization: `Bearer ${token}` }
      });
      
      // Ordenar por riqueza (maior montante primeiro) logo na chegada dos dados
      const ordenadas = res.data.sort((a, b) => b.montante - a.montante);
      setContas(ordenadas);
    } catch (err) {
      console.error("Erro ao carregar contas", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchContas(); }, []);

  const alternarEstadoConta = async (id, estadoAtual) => {
    const acao = estadoAtual ? "congelar" : "reabrir";
    if (!window.confirm(`Tens a certeza que desejas ${acao} esta conta?`)) return;

    try {
      const token = localStorage.getItem('token');
      await axios.put(`https://localhost:7221/api/Admin/contas/${id}/estado`, 
        { novoEstado: !estadoAtual }, 
        { headers: { Authorization: `Bearer ${token}` } }
      );
      
      setContas(contas.map(c => c.idConta === id ? { ...c, isAberta: !estadoAtual } : c));
    } catch (err) {
      alert("Erro ao alterar estado da conta.");
    }
  };

  // Filtro de busca (por titular ou nome da conta)
  const contasFiltradas = contas.filter(c => 
    c.nomeCliente.toLowerCase().includes(busca.toLowerCase()) ||
    c.nomeConta.toLowerCase().includes(busca.toLowerCase())
  );

  // Cálculos para os cartões de resumo
  const totalGeral = contas.reduce((acc, c) => acc + c.montante, 0);
  const contasCongeladas = contas.filter(c => !c.isAberta).length;

  if (loading) return (
    <div className="loading-state">
      <Loader2 className="spin-animation" size={40} />
      <p>Sincronizando cofres...</p>
    </div>
  );

  return (
    <div className="page-content">
      <div className="content-header">
        <div>
          <h1>Gestão de Contas Bancárias</h1>
          <p>Visão global de saldos e controlo de liquidez.</p>
        </div>
        
        {/* Barra de Pesquisa Rápida */}
        <div className="search-box">
          <Search size={18} />
          <input 
            type="text" 
            placeholder="Procurar titular ou conta..." 
            value={busca}
            onChange={(e) => setBusca(e.target.value)}
          />
        </div>
      </div>

      {/* Cartões de Estatísticas Rápidas */}
      <div className="stats-grid" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '20px', marginBottom: '30px' }}>
        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(16, 185, 129, 0.1)', color: '#10B981' }}><Wallet size={24} /></div>
          <div className="stat-info">
            <span className="stat-label">Capital Total</span>
            <span className="stat-value">{totalGeral.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })}</span>
          </div>
        </div>
        
        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(59, 130, 246, 0.1)', color: '#3b82f6' }}><Landmark size={24} /></div>
          <div className="stat-info">
            <span className="stat-label">Contas Ativas</span>
            <span className="stat-value">{contas.length - contasCongeladas}</span>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(239, 68, 68, 0.1)', color: '#ef4444' }}><ShieldAlert size={24} /></div>
          <div className="stat-info">
            <span className="stat-label">Congeladas</span>
            <span className="stat-value">{contasCongeladas}</span>
          </div>
        </div>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Conta / Tipo</th>
              <th>Titular</th>
              <th style={{ textAlign: 'right' }}>Saldo</th>
              <th>Estado</th>
              <th style={{ textAlign: 'center' }}>Ações de Segurança</th>
            </tr>
          </thead>
          <tbody>
            {contasFiltradas.map(c => (
              <tr key={c.idConta}>
                <td>
                  <div style={{ display: 'flex', flexDirection: 'column' }}>
                    <span style={{ fontWeight: '600' }}>{c.nomeConta}</span>
                    <small style={{ color: '#888' }}>ID: #{c.idConta}</small>
                  </div>
                </td>
                <td><strong>{c.nomeCliente}</strong></td>
                <td style={{ 
                  textAlign: 'right', 
                  color: c.montante >= 0 ? '#10B981' : '#EF4444', 
                  fontWeight: 'bold',
                  fontFamily: 'monospace',
                  fontSize: '1.1rem'
                }}>
                  {c.montante.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })}
                </td>
                <td>
                  <span className={`status-badge ${c.isAberta ? 'active' : 'blocked'}`}>
                    {c.isAberta ? 'Ativa' : 'Congelada'}
                  </span>
                </td>
                <td style={{ textAlign: 'center' }}>
                  <button 
                    onClick={() => alternarEstadoConta(c.idConta, c.isAberta)}
                    className={`btn-action ${c.isAberta ? 'btn-block' : 'btn-activate'}`}
                    style={{ minWidth: '120px' }}
                  >
                    {c.isAberta ? <><Lock size={14}/> Congelar</> : <><Unlock size={14}/> Reabrir</>}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {contasFiltradas.length === 0 && (
          <div style={{ textAlign: 'center', padding: '30px', color: '#666' }}>
            Nenhuma conta encontrada para "{busca}".
          </div>
        )}
      </div>
    </div>
  );
};

export default Contas;