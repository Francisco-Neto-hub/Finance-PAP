using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Models
{
    /// <summary>
    /// Representa uma categoria de movimento (ex: Alimentação, Lazer, Salário).
    /// </summary>
    public class Categoria
    {
        /// <summary> Identificador único da categoria. </summary>
        public int IdCategoria { get; set; }

        /// <summary> Nome da categoria. </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary> Define se a categoria está disponível para seleção. </summary>
        public bool Ativo { get; set; } = true;
    }
}
