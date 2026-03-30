namespace Finance.API.DTOs
{
    public class ClienteAdminCreateDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telemovel { get; set; }
        public DateTime DataNasc { get; set; }
        public string Password { get; set; }
        public int IdPerfil { get; set; }
    }

    public class ClienteAdminUpdateDTO
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telemovel { get; set; }
        public int IdPerfil { get; set; }
    }

    public class ClienteAdminReadDTO
    {
        public int IdCliente { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telemovel { get; set; }
        public DateTime DataNasc { get; set; }
        public int IdPerfil { get; set; }
        public bool IsAtivo { get; set; }
        public bool IsExcluido { get; set; }
    }

    // DTOs para os comandos do Admin
    public class AlterarEstadoDTO
    {
        public bool NovoEstado { get; set; } // true para Ativar/Abrir, false para Bloquear/Fechar
    }

    public class CategoriaAdminDTO
    {
        public string Nome { get; set; }
    }

    // DTOs para a visualização Global (Read)
    public class ContaGlobalReadDTO
    {
        public int IdConta { get; set; }
        public string NomeConta { get; set; }
        public decimal Montante { get; set; }
        public bool IsAberta { get; set; }
        public string NomeCliente { get; set; } // O Admin precisa de saber de quem é!
    }

    public class TransacaoGlobalReadDTO
    {
        public int IdTransacao { get; set; }
        public string NomeCliente { get; set; }
        public string NomeConta { get; set; }
        public string TipoMovimento { get; set; }
        public decimal ValorTransacao { get; set; }
        public DateTime DataTransacao { get; set; }
        public bool IsConcluida { get; set; }
    }
}