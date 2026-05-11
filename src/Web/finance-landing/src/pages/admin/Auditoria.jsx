import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { ShieldAlert, ArrowRight, User, Calendar } from 'lucide-react';

const Auditoria = () => {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        const token = localStorage.getItem('token');
        const response = await axios.get('https://localhost:7221/api/Auditoria/saldos', {
          headers: { Authorization: `Bearer ${token}` }
        });
        setLogs(response.data);
      } catch (err) {
        console.error("Erro ao carregar auditoria:", err);
      } finally {
        setLoading(false);
      }
    };
    fetchLogs();
  }, []);

  const formatCurrency = (value) => {
    return new Intl.NumberFormat('pt-PT', { style: 'currency', currency: 'EUR' }).format(value);
  };

  return (
    <div className="page-content">
      <div className="content-header">
        <h1>Auditoria de Sistema</h1>
        <p>Registo histórico de todas as alterações de saldo (Triggers SQL).</p>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Data/Hora</th>
              <th>Conta</th>
              <th>Fluxo de Saldo</th>
              <th>Utilizador/Processo</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan="4" style={{ textAlign: 'center' }}>A carregar registos...</td></tr>
            ) : (
              logs.map(log => (
                <tr key={log.idLog}>
                  <td style={{ fontSize: '0.85rem' }}>
                    <div className="flex-align">
                      <Calendar size={14} className="text-blue" />
                      {new Date(log.dataAlteracao).toLocaleString('pt-PT')}
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
                      <User size={14} />
                      {log.usuario}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Auditoria;