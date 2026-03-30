namespace Finance.API.DTOs
{
    /// <summary>
    /// Objeto utilizado para registar um novo movimento financeiro.
    /// </summary>
    public class TransacaoCreateDTO
    {
        /// <summary>
        /// O ID da conta de onde o dinheiro vai sair (em caso de despesa/transferência) ou entrar (receita).
        /// </summary>
        public int IdContaOrigem { get; set; }

        /// <summary>
        /// O ID da conta destino. Obrigatório APENAS se o IdTipo for 3 (Transferência). Para receitas ou despesas, enviar nulo.
        /// </summary>
        public int? IdContaDestino { get; set; }

        /// <summary>
        /// O ID da categoria da transação (ex: 1 - Salário, 4 - Supermercado).
        /// </summary>
        public int IdCategoria { get; set; }

        /// <summary>
        /// O tipo de movimento: 1 para Receita, 2 para Despesa, 3 para Transferência.
        /// </summary>
        public int IdTipo { get; set; }

        /// <summary>
        /// O montante da transação. Tem de ser obrigatoriamente superior a zero.
        /// </summary>
        public decimal ValorTransacao { get; set; }

        /// <summary>
        /// Uma breve descrição ou título para o movimento (ex: "Jantar com amigos").
        /// </summary>
        public string NomeTransacao { get; set; }

        public DateTime DataTransacao { get; set; }
    }

    // O que devolvemos no Extrato (READ)
    public class TransacaoReadDTO
    {
        public int IdTransacao { get; set; }
        public string TipoMovimento { get; set; } // Nome do tipo (ex: "Despesa")
        public string Categoria { get; set; } // Nome da categoria
        public decimal ValorTransacao { get; set; }
        public string NomeTransacao { get; set; }
        public DateTime DataTransacao { get; set; }
        public bool IsConcluida { get; set; } // Ajuda o Frontend a saber se está anulada ou não
    }

    // O que o utilizador envia para EDITAR (Segurança: Não muda conta nem valor)
    public class TransacaoUpdateDTO
    {
        public int? IdCategoria { get; set; }
        public string NomeTransacao { get; set; }
        public DateTime DataTransacao { get; set; }
    }
}