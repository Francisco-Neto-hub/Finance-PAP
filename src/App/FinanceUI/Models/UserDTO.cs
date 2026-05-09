namespace FinanceUI.Models
{
    public class UserDTO
    {
        // DTO para atualização de dados básicos
        public class UserUpdateDTO
        {
            public string Nome { get; set; }
            public string Email { get; set; }
            public string Telemovel { get; set; }
            public DateTime DataNasc { get; set; }
        }

        // DTO para mudança de password (logado)
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

        // DTO para recuperação (não logado)
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
}
