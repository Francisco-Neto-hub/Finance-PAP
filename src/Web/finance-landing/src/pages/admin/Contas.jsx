import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Loader2, Lock, Unlock } from 'lucide-react';

const Contas = () => {
  const [contas, setContas] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchContas = async () => {
    try {
      const token = localStorage.getItem('token');
      const res = await axios.get('https://localhost:7221/api/Admin/contas', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setContas(res.data);
    } catch (err) {
      console.error("Erro ao carregar contas", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchContas(); }, []);

  const alternarEstadoConta = async (id, estadoAtual) => {
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

  if (loading) return <div className="loading-state"><Loader2 className="spin-animation" /></div>;

  return (
    <div className="page-content">
      <div className="content-header">
        <h1>Gestão de Contas Bancárias</h1>
        <p>Visão global de saldos e bloqueio de fundos (ordenado por riqueza).</p>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>ID Conta</th>
              <th>Titular</th>
              <th>Saldo</th>
              <th>Estado</th>
              <th>Ação de Segurança</th>
            </tr>
          </thead>
          <tbody>
            {contas.map(c => (
              <tr key={c.idConta}>
                <td>#{c.idConta} ({c.nomeConta})</td>
                <td><strong>{c.nomeCliente}</strong></td>
                <td style={{ color: '#10B981', fontWeight: 'bold' }}>
                  {c.montante.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })}
                </td>
                <td>
                  <span className={`status-badge ${c.isAberta ? 'active' : 'blocked'}`}>
                    {c.isAberta ? 'Ativa' : 'Congelada'}
                  </span>
                </td>
                <td>
                  <button 
                    onClick={() => alternarEstadoConta(c.idConta, c.isAberta)}
                    className={`btn-action ${c.isAberta ? 'btn-block' : 'btn-activate'}`}
                  >
                    {c.isAberta ? <><Lock size={16}/> Congelar</> : <><Unlock size={16}/> Reabrir</>}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
export default Contas;