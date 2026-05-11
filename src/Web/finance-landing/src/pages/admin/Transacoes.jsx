import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Loader2, AlertOctagon } from 'lucide-react';

const Transacoes = () => {
  const [transacoes, setTransacoes] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchTransacoes = async () => {
    try {
      const token = localStorage.getItem('token');
      const res = await axios.get('https://localhost:7221/api/Admin/transacoes', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setTransacoes(res.data);
    } catch (err) { console.error(err); } 
    finally { setLoading(false); }
  };

  useEffect(() => { fetchTransacoes(); }, []);

  const forcarAnulacao = async (idTransacao) => {
    if (!window.confirm("ATENÇÃO: Tens a certeza que queres reverter fundos desta transação?")) return;
    
    try {
      const token = localStorage.getItem('token');
      await axios.put(`https://localhost:7221/api/Admin/transacoes/${idTransacao}/forcar-anulacao`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      });
      alert("Transação anulada e saldo corrigido!");
      fetchTransacoes(); // Recarrega para mostrar estado atualizado
    } catch (err) {
      alert(err.response?.data?.erro || "Erro ao anular transação.");
    }
  };

  if (loading) return <div className="loading-state"><Loader2 className="spin-animation" /></div>;

  return (
    <div className="page-content">
      <div className="content-header">
        <h1>Auditoria de Transações</h1>
        <p>Fluxo global do sistema. O modo de anulação ignora restrições do cliente.</p>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Data</th>
              <th>Cliente / Conta</th>
              <th>Tipo</th>
              <th>Valor</th>
              <th>Estado</th>
              <th>Ações Admin</th>
            </tr>
          </thead>
          <tbody>
            {transacoes.map(t => (
              <tr key={t.idTransacao} style={{ opacity: t.isConcluida ? 1 : 0.5 }}>
                <td>{new Date(t.dataTransacao).toLocaleString('pt-PT')}</td>
                <td><strong>{t.nomeCliente}</strong> <br/> <small>{t.nomeConta}</small></td>
                <td>{t.tipoMovimento}</td>
                <td style={{ fontWeight: 'bold' }}>
                  {t.valorTransacao.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })}
                </td>
                <td>
                  <span className={`status-badge ${t.isConcluida ? 'active' : 'blocked'}`}>
                    {t.isConcluida ? 'Concluída' : 'Anulada/Pendente'}
                  </span>
                </td>
                <td>
                  {t.isConcluida && (
                    <button onClick={() => forcarAnulacao(t.idTransacao)} className="btn-action btn-block">
                      <AlertOctagon size={16}/> Reverter Fundos
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
export default Transacoes;