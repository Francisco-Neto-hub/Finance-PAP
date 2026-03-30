namespace Finance.API.DTOs
{
    // === PARA O UTILIZADOR (USER) ===
    public class ContaUserReadDTO
    {
        public int IdConta { get; set; }
        public string NomeConta { get; set; }
        public decimal Montante { get; set; }
        public bool IsAberta { get; set; }
        public DateTime DataInicio { get; set; }
    }

    /// <summary>
    /// Objeto para a abertura de uma nova conta bancária associada ao contrato do cliente.
    /// </summary>
    public class ContaUserCreateDTO
    {
        /// <summary>
        /// O nome personalizado da nova conta (ex: "Conta Poupança Férias").
        /// </summary>
        public string NomeConta { get; set; }

        /// <summary>
        /// O depósito inicial da conta. Pode ser zero.
        /// </summary>
        public decimal Montante { get; set; }
    }

    public class ContaUserUpdateDTO
    {
        public string NomeConta { get; set; }
    }

    // === PARA O ADMINISTRADOR (ADMIN) ===
    public class ContaAdminReadDTO : ContaUserReadDTO
    {
        public int IdCliente { get; set; } // O Admin vê um campo extra: a quem pertence a conta
    }
}
