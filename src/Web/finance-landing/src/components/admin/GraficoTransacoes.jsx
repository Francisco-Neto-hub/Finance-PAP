import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { Loader2 } from 'lucide-react';

const CustomTooltip = ({ active, payload, label }) => {
  if (active && payload && payload.length) {
    return (
      <div className="custom-tooltip" style={{
        background: '#141414', border: '1px solid #2a2a2a',
        padding: '10px 15px', borderRadius: '8px', color: '#fff'
      }}>
        <p style={{ margin: 0, fontWeight: 'bold', color: '#a0a0a0', marginBottom: '5px' }}>{label}</p>
        <p style={{ margin: 0, color: '#00C2FF', fontSize: '1.2rem', fontWeight: 'bold' }}>
          {payload[0].value.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' })}
        </p>
      </div>
    );
  }
  return null;
};

const GraficoTransacoes = () => {
  const [dadosGrafico, setDadosGrafico] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const processarTransacoes = async () => {
      try {
        const token = localStorage.getItem('token');
        // Chama o endpoint que já tens criado!
        const response = await axios.get('https://localhost:7221/api/Admin/transacoes', {
          headers: { Authorization: `Bearer ${token}` }
        });

        const transacoes = response.data;

        // 1. Criar array base com os 12 meses a zeros
        const mesesNomes = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
        const volumeMensal = mesesNomes.map(m => ({ mes: m, volume: 0 }));

        // 2. Preencher com os dados da API
        transacoes.forEach(t => {
          // Apenas somamos transações concluídas
          if (t.isConcluida) {
            const data = new Date(t.dataTransacao);
            const mesIndex = data.getMonth(); // Devolve de 0 a 11
            
            // Usamos Math.abs para somar o volume de dinheiro movimentado, 
            // independentemente de ser levantamento (-) ou depósito (+)
            volumeMensal[mesIndex].volume += Math.abs(t.valorTransacao);
          }
        });

        setDadosGrafico(volumeMensal);
      } catch (err) {
        console.error("Erro ao carregar dados do gráfico", err);
      } finally {
        setLoading(false);
      }
    };

    processarTransacoes();
  }, []);

  if (loading) {
    return (
      <div className="dashboard-widget" style={{ height: '400px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Loader2 className="spin-animation" color="#00C2FF" size={30} />
      </div>
    );
  }

  return (
    <div className="dashboard-widget" style={{ height: '400px', padding: '20px' }}>
      <div className="widget-header">
        <h3>Volume Financeiro Mensal (Ano Atual)</h3>
      </div>
      
      <ResponsiveContainer width="100%" height="85%">
        <AreaChart data={dadosGrafico} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
          <defs>
            <linearGradient id="colorVolume" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor="#00C2FF" stopOpacity={0.8}/>
              <stop offset="95%" stopColor="#00C2FF" stopOpacity={0}/>
            </linearGradient>
          </defs>

          <CartesianGrid strokeDasharray="3 3" stroke="#2a2a2a" vertical={false} />
          
          <XAxis dataKey="mes" stroke="#888" tick={{ fill: '#888' }} axisLine={false} tickLine={false} />
          <YAxis 
            stroke="#888" 
            tick={{ fill: '#888' }} 
            axisLine={false} 
            tickLine={false} 
            tickFormatter={(value) => value >= 1000 ? `${(value / 1000).toFixed(1)}k` : value} 
          />
          
          <Tooltip content={<CustomTooltip />} cursor={{ stroke: '#2a2a2a', strokeWidth: 2 }} />
          
          <Area 
            type="monotone" 
            dataKey="volume" 
            stroke="#00C2FF" 
            strokeWidth={3}
            fillOpacity={1} 
            fill="url(#colorVolume)" 
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
};

export default GraficoTransacoes;