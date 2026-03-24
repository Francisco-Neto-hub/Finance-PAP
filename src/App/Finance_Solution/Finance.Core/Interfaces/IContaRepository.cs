using Finance.Core.DTOs;
using Finance.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Interfaces
{
    public interface IContaRepository
    {
        Task<IEnumerable<ResumoContaDTO>> ObterResumoContasAsync();
        Task<bool> CriarAsync(Conta conta);
    }
}
