import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { 
  Loader2, AlertOctagon, ShieldAlert, ArrowRight, 
  User, Calendar, History, ArrowDownUp 
} from 'lucide-react';

const AuditoriaTransacoes = () => {
  const [activeTab, setActiveTab] = useState('fluxo'); // 'fluxo' ou 'logs'
  const [transacoes, setTransacoes] = useState([]);
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchData = async () => {
    setLoading(true);
    const token = localStorage.getItem('token');
    try {
      if (activeTab === 'fluxo') {
        const res = await axios.get('https://localhost:7221/api/Admin/transacoes', {
          headers: { Authorization: `Bearer ${token}` }
        });
        setTransacoes(res.data);
      } else {
        const res = await axios.get('https://localhost:7221/api/Auditoria/saldos', {
          headers: { Authorization: `Bearer ${token}` }
        });
        setLogs(res.data);
      }
    } catch (err) {
      console.error("Erro na extração de dados:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, [activeTab]);

  const forcarAnulacao = async (idTransacao) => {
    if (!window.confirm("CUIDADO: Esta ação ignora regras de negócio e reverte o saldo. Continuar?")) return;
    try {
      const token = localStorage.getItem('token');
      await axios.put(`https://localhost:7221/api/Admin/transacoes/${idTransacao}/forcar-anulacao`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      });
      alert("Operação revertida com sucesso!");
      fetchData();
    } catch (err) {
      alert(err.response?.data?.erro || "Erro na reversão.");
    }
  };

  const formatCurrency = (val) => new Intl.NumberFormat('pt-PT', { style: 'currency', currency: 'EUR' }).format(val);

  return (
    <div className="page-content">
      <div className="content-header">
        <div>
          <h1>Centro de Auditoria</h1>
          <p>Monitorização de integridade financeira e histórico de segurança.</p>
        </div>
        
        {/* NAVEGAÇÃO POR TABS */}
        <div className="tabs-container">
          <button 
            className={`tab-btn ${activeTab === 'fluxo' ? 'active' : ''}`}
            onClick={() => setActiveTab('fluxo')}
          >
            <ArrowDownUp size={18} /> Fluxo de Transações
          </button>
          <button 
            className={`tab-btn ${activeTab === 'logs' ? 'active' : ''}`}
            onClick={() => setActiveTab('logs')}
          >
            <History size={18} /> Logs de Saldo (Trigger SQL)
          </button>
        </div>
      </div>

      {loading ? (
        <div className="loading-state"><Loader2 className="spin-animation" /> Descodificando registos...</div>
      ) : (
        <div className="table-container">
          {activeTab === 'fluxo' ? (
            /* --- TABELA DE TRANSAÇÕES --- */
            <table>
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Cliente / Conta</th>
                  <th>Tipo</th>
                  <th>Valor</th>
                  <th>Estado</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {transacoes.map(t => (
                  <tr key={t.idTransacao} className={!t.isConcluida ? 'row-disabled' : ''}>
                    <td>{new Date(t.dataTransacao).toLocaleString('pt-PT')}</td>
                    <td><strong>{t.nomeCliente}</strong> <br/><small>{t.nomeConta}</small></td>
                    <td><span className="type-tag">{t.tipoMovimento}</span></td>
                    <td className="font-mono">{formatCurrency(t.valorTransacao)}</td>
                    <td>
                      <span className={`status-badge ${t.isConcluida ? 'active' : 'blocked'}`}>
                        {t.isConcluida ? 'Concluída' : 'Anulada'}
                      </span>
                    </td>
                    <td>
                      {t.isConcluida && (
                        <button onClick={() => forcarAnulacao(t.idTransacao)} className="btn-action btn-block">
                          <AlertOctagon size={14}/> Reverter
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            /* --- TABELA DE LOGS DE AUDITORIA --- */
            <table>
              <thead>
                <tr>
                  <th>Data/Hora</th>
                  <th>Conta</th>
                  <th>Variação de Saldo</th>
                  <th>Utilizador/Processo</th>
                </tr>
              </thead>
              <tbody>
                {logs.map(log => (
                  <tr key={log.idLog}>
                    <td>
                      <div className="flex-align text-blue">
                        <Calendar size={14} /> {new Date(log.dataAlteracao).toLocaleString('pt-PT')}
                      </div>
                    </td>
                    <td><strong>{log.nomeConta}</strong> <small>(#{log.idConta})</small></td>
                    <td>
                      <div className="balance-flow">
                        <span className="old-balance">{formatCurrency(log.saldoAntigo)}</span>
                        <ArrowRight size={14} />
                        <span className={`new-balance ${log.saldoNovo >= log.saldoAntigo ? 'text-green' : 'text-red'}`}>
                          {formatCurrency(log.saldoNovo)}
                        </span>
                      </div>
                    </td>
                    <td>
                      <div className="flex-align">
                        <User size={14} /> {log.usuario}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  );
};

export default AuditoriaTransacoes;