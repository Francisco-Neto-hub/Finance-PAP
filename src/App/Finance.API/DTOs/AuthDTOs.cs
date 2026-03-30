namespace Finance.API.DTOs
{
    /// <summary>
    /// Objeto que transporta as credenciais do utilizador para o Login.
    /// </summary>
    public class LoginRequestDTO
    {
        /// <summary>
        /// O endereço de email registado na plataforma.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// A palavra-passe do utilizador (será encriptada no processo de verificação).
        /// </summary>
        public required string Password { get; set; }
    }
    /// <summary>
    /// Objeto com os dados necessários para criar um novo cliente, contrato e primeira conta.
    /// </summary>
    public class RegistoRequestDTO
    {
        /// <summary>
        /// O nome completo do novo cliente.
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// O endereço de email (deve ser único no sistema).
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// O número de telemóvel para contacto.
        /// </summary>
        public string Telemovel { get; set; }

        /// <summary>
        /// A data de nascimento do cliente (Formato: AAAA-MM-DD).
        /// </summary>
        public DateTime DataNasc { get; set; }

        /// <summary>
        /// A palavra-passe pretendida (será guardada em formato Hash SHA-256).
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Objeto com os dados necessários para altera a password.
    /// </summary>
    public class MudarPasswordDTO
    {
        /// <summary>
        /// A palavra-passe antiga
        /// </summary>
        public string PasswordAntiga { get; set; }

        /// <summary>
        /// A nova palavra-passe pretendida (será guardada em formato Hash SHA-256).
        /// </summary>
        public string PasswordNova { get; set; }
    }

    /// <summary>
    /// Objeto com os dados para recuperar a password sem estar logado.
    /// </summary>
    public class RecuperarPasswordDTO
    {
        /// <summary>
        /// O email da conta.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// O telemóvel registado na conta (funciona como medida de segurança).
        /// </summary>
        public string Telemovel { get; set; }

        /// <summary>
        /// A nova palavra-passe pretendida.
        /// </summary>
        public string NovaPassword { get; set; }
    }
}
