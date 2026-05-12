import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { UserCheck, UserX, Edit, Loader2, X, Save, Key, Phone, Mail, User, Calendar, Shield } from 'lucide-react';

const Clientes = () => {
  const [clientes, setClientes] = useState([]);
  const [loading, setLoading] = useState(true);

  // --- ESTADOS PARA EDIÇÃO ---
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedCliente, setSelectedCliente] = useState(null);
  const [editData, setEditData] = useState({
    nome: '',
    email: '',
    telemovel: '',
    dataNasc: '',
    idPerfil: 2,
    password: '' 
  });

  // 1. Carregar a lista inicial
  const carregarClientes = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get('https://localhost:7221/api/Admin/listar_clientes', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setClientes(response.data);
    } catch (err) {
      console.error("Erro ao carregar clientes", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregarClientes();
  }, []);

  // --- 2. NOVA LOGICA: BLOQUEAR / ATIVAR ---
  const alternarEstadoCliente = async (id, estadoAtual) => {
    const acao = estadoAtual ? "bloquear" : "ativar";
    if (!window.confirm(`Tens a certeza que desejas ${acao} este acesso?`)) return;

    try {
      const token = localStorage.getItem('token');
      // Chamada ao teu endpoint [HttpPut("clientes/{idCliente}/estado")]
      await axios.put(`https://localhost:7221/api/Admin/clientes/${id}/estado`, 
        { novoEstado: !estadoAtual }, 
        { headers: { Authorization: `Bearer ${token}` } }
      );

      // Atualização "Optimistic UI" (atualiza logo na lista sem recarregar tudo)
      setClientes(clientes.map(c => 
        c.idCliente === id ? { ...c, isAtivo: !estadoAtual } : c
      ));

      alert(`Utilizador ${estadoAtual ? 'bloqueado' : 'ativado'} com sucesso.`);
    } catch (err) {
      alert("Erro ao alterar o estado de acesso do cliente.");
    }
  };

  // 3. Lógica para Abrir Modal de Edição
  const handleEditClick = (cliente) => {
    setSelectedCliente(cliente);
    const dataFormatada = cliente.dataNasc ? cliente.dataNasc.split('T')[0] : '';
    setEditData({
      nome: cliente.nome,
      email: cliente.email,
      telemovel: cliente.telemovel || '',
      idPerfil: cliente.idPerfil,
      dataNasc: dataFormatada,
      password: '' 
    });
    setIsModalOpen(true);
  };

  // 4. Lógica para Guardar Alterações
  const handleSave = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      const id = selectedCliente.idCliente;

      // CHAMADA 1: Dados Gerais
      await axios.put(`https://localhost:7221/api/Admin/clientes/${id}/atualizar_dados`, editData, {
        headers: { Authorization: `Bearer ${token}` }
      });

      // CHAMADA 2: Perfil
      await axios.put(`https://localhost:7221/api/Admin/clientes/${id}/perfil`, 
        editData.idPerfil, 
        { 
          headers: { 
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json' 
          } 
        }
      );

      setIsModalOpen(false);
      carregarClientes(); 
      alert("Protocolo e nível de acesso atualizados!");
    } catch (err) {
      const msg = err.response?.data?.mensagem || "Erro na operação";
      alert(msg);
    }
  };

  if (loading) return (
    <div className="loading-state">
      <Loader2 className="spin-animation" /> Acedendo ao terminal de segurança...
    </div>
  );

  return (
    <div className="page-content">
      <div className="content-header">
        <h1>Gestão de Clientes</h1>
        <p>Controlo total sobre as credenciais e estados de acesso.</p>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Nome / Email</th>
              <th>Estado</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {clientes.map(c => (
              <tr key={c.idCliente}>
                <td><code className="text-blue">#{c.idCliente}</code></td>
                <td>
                  <div className="user-info">
                    <strong>{c.nome}</strong>
                    <br />
                    <small style={{ color: '#888' }}>{c.email}</small>
                  </div>
                </td>
                <td>
                  <span className={`status-badge ${c.isAtivo ? 'active' : 'blocked'}`}>
                    {c.isAtivo ? 'Ativo' : 'Bloqueado'}
                  </span>
                </td>
                <td>
                  <div style={{ display: 'flex', gap: '10px' }}>
                    <button 
                      className="btn-action btn-edit" 
                      onClick={() => handleEditClick(c)}
                      title="Editar Dados"
                    >
                      <Edit size={16} />
                    </button>
                    
                    <button 
                      className={`btn-action ${c.isAtivo ? 'btn-block' : 'btn-activate'}`}
                      onClick={() => alternarEstadoCliente(c.idCliente, c.isAtivo)}
                      title={c.isAtivo ? "Bloquear Acesso" : "Ativar Acesso"}
                    >
                      {c.isAtivo ? <UserX size={16} /> : <UserCheck size={16} />}
                      {c.isAtivo ? ' Bloquear' : ' Ativar'}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* --- MODAL DE EDIÇÃO --- */}
      {isModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content admin-edit-modal">
            <div className="modal-header">
              <h3><Edit size={20} color="#00C2FF" /> Alterar Dados: {selectedCliente?.nome}</h3>
              <button className="close-btn" onClick={() => setIsModalOpen(false)}><X /></button>
            </div>

            <form onSubmit={handleSave} className="edit-form">
              <div className="input-group">
                <label><User size={14} /> Nome Completo</label>
                <input 
                  type="text" 
                  value={editData.nome} 
                  onChange={(e) => setEditData({...editData, nome: e.target.value})} 
                  required 
                />
              </div>

              <div className="input-group">
                <label><Mail size={14} /> Email de Acesso</label>
                <input 
                  type="email" 
                  value={editData.email} 
                  onChange={(e) => setEditData({...editData, email: e.target.value})} 
                  required 
                />
              </div>

              <div className="input-group">
                <label><Phone size={14} /> Telemóvel</label>
                <input 
                  type="text" 
                  value={editData.telemovel} 
                  onChange={(e) => setEditData({...editData, telemovel: e.target.value})} 
                />
              </div>

              <div className="input-group">
                <label><Calendar size={14} /> Data de Nascimento</label>
                <input 
                  type="date" 
                  value={editData.dataNasc} 
                  onChange={(e) => setEditData({...editData, dataNasc: e.target.value})} 
                  required 
                />
              </div>

              <div className="input-group">
              <label><Shield size={14} /> Nível de Permissão</label>
              <select 
                value={editData.idPerfil} 
                onChange={(e) => setEditData({...editData, idPerfil: parseInt(e.target.value)})}
                className="admin-select"
              >
                <option value={2}>Cliente Normal (Acesso App)</option>
                <option value={1}>Administrador (Acesso Total)</option>
              </select>
              </div>

              <div className="password-reset-box">
                <label><Key size={14} /> Redefinir Password</label>
                <input 
                  type="password" 
                  placeholder="Deixe vazio para não alterar"
                  value={editData.password} 
                  onChange={(e) => setEditData({...editData, password: e.target.value})} 
                />
              </div>

              <div className="modal-actions">
                <button type="button" className="btn-cancel" onClick={() => setIsModalOpen(false)}>Cancelar</button>
                <button type="submit" className="btn-save">
                   <Save size={18} /> Gravar Alterações
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Clientes;