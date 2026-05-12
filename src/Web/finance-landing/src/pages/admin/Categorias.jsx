import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Loader2, Plus, Trash2, Edit } from 'lucide-react';

const Categorias = () => {
  const [categorias, setCategorias] = useState([]);
  const [novaCategoria, setNovaCategoria] = useState("");

  const fetchCategorias = async () => {
    try {
      const token = localStorage.getItem('token');
      // Assume que tens um GET /api/Admin/categorias ou GET /api/Categorias
      const res = await axios.get('https://localhost:7221/api/Categorias', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setCategorias(res.data);
    } catch (err) { console.error(err); }
  };

  useEffect(() => { fetchCategorias(); }, []);

  const criarCategoria = async (e) => {
    e.preventDefault();
    try {
      const token = localStorage.getItem('token');
      await axios.post('https://localhost:7221/api/Admin/categorias', { nome: novaCategoria }, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setNovaCategoria("");
      fetchCategorias();
    } catch (err) { alert("Erro ao criar."); }
  };

  const apagarCategoria = async (id) => {
    if (!window.confirm("Apagar categoria?")) return;
    try {
      const token = localStorage.getItem('token');
      await axios.delete(`https://localhost:7221/api/Admin/categorias/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      fetchCategorias();
    } catch (err) {
      alert(err.response?.data?.mensagem || "Erro ao apagar. Pode estar em uso.");
    }
  };

  return (
    <div className="page-content">
      <div className="content-header">
        <h1>Gestão de Categorias</h1>
      </div>

      <div className="dashboard-widget" style={{ marginBottom: '20px' }}>
        <form onSubmit={criarCategoria} style={{ display: 'flex', gap: '10px' }}>
          <input 
            type="text" value={novaCategoria} onChange={(e) => setNovaCategoria(e.target.value)}
            placeholder="Nome da nova categoria..." required
            style={{ padding: '10px', borderRadius: '8px', background: '#1a1a1a', color: 'white', border: '1px solid #333', flex: 1 }}
          />
          <button type="submit" className="btn-save"><Plus size={18}/> Adicionar</button>
        </form>
      </div>

      <div className="table-container">
        <table>
          <thead><tr><th>ID</th><th>Nome da Categoria</th><th>Ações</th></tr></thead>
          <tbody>
            {categorias.map(c => (
              <tr key={c.idCategoria}>
                <td>#{c.idCategoria}</td>
                <td>{c.nome}</td>
                <td>
                  <button onClick={() => apagarCategoria(c.idCategoria)} className="btn-action btn-block">
                    <Trash2 size={16}/> Apagar
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
export default Categorias;