using System;
using System.Collections.Generic;
using System.Text;

namespace Gestao.Core.Models
{
    public class VersaoSoftware
    {
        public int IdVersao { get; set; }
        public string Versao { get; set; } = string.Empty;
        public DateTime DataRelease { get; set; }
        public string UrlDownload { get; set; } = string.Empty;
        public string NotasVersao { get; set; } = string.Empty;
    }
}
