import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { MessageSquare, CheckCircle, Clock, Loader2 } from 'lucide-react';

const Tickets = () => {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchTickets = async () => {
    try {
      const token = localStorage.getItem('token');
      // CORREÇÃO: Porta 7221 para manter consistência com o backend
      const response = await axios.get('https://localhost:7221/api/Admin/tickets-pendentes', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setTickets(response.data);
    } catch (err) {
      console.error("Erro ao procurar tickets", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { 
    fetchTickets(); 
  }, []);

  const marcarComoResolvido = async (id) => {
    try {
      const token = localStorage.getItem('token');
      await axios.put(`https://localhost:7221/api/Admin/tickets/${id}/resolver`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      });
      fetchTickets(); // Atualiza a lista automaticamente após resolver
    } catch (err) {
        alert("Erro ao atualizar ticket");
    }
  };

  // 1. Mostrar um loader enquanto vai buscar os dados
  if (loading) return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '50vh', color: '#888' }}>
      <Loader2 className="spin-animation" size={32} style={{ marginBottom: '1rem' }} /> 
      A carregar pedidos de suporte...
    </div>
  );

  return (
    <div className="page-content">
      <div className="content-header">
        <h1>Centro de Suporte</h1>
        <p>Mensagens e pedidos de ajuda dos utilizadores.</p>
      </div>

      {/* 2. Mensagem caso não existam tickets */}
      {tickets.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '4rem 2rem', background: '#141414', borderRadius: '12px', border: '1px solid #2a2a2a' }}>
          <MessageSquare size={48} color="#333" style={{ marginBottom: '1rem' }} />
          <h3 style={{ color: '#888', marginBottom: '0.5rem' }}>Caixa de entrada limpa!</h3>
          <p style={{ color: '#555' }}>Não há pedidos de suporte pendentes neste momento.</p>
        </div>
      ) : (
        <div className="tickets-grid">
          {tickets.map(t => (
            <div key={t.idTicket} className={`ticket-card ${t.isResolvido ? 'resolved' : 'pending'}`}>
              <div className="ticket-badge">
                {t.isResolvido ? <CheckCircle size={14}/> : <Clock size={14}/>}
                {t.isResolvido ? 'Resolvido' : 'Pendente'}
              </div>
              
              <h3>{t.assunto}</h3>
              <p className="ticket-msg">"{t.mensagem}"</p>
              
              <div className="ticket-footer">
                <span>ID Cliente: #{t.idCliente}</span>
                {!t.isResolvido && (
                  <button onClick={() => marcarComoResolvido(t.idTicket)} className="btn-resolve">
                    Marcar como Resolvido
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Tickets;