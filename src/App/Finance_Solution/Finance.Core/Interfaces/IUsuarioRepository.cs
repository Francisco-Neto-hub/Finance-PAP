using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<int?> ValidarLoginAsync(string email, string password);
    }
}
