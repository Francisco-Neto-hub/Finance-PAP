using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using Gestao.Core.Interfaces;
using Gestao.Core.Models;

namespace Gestao.Core.Services
{
    public class VersionService : IVersionService
    {
        private readonly DbConnectionFactory _dbFactory;

        public VersionService(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public VersaoSoftware ObterUltimaVersao()
        {
            using var db = _dbFactory.CreateConnection();
            // O 'as' garante que o SQL entrega o nome que o C# espera
            string sql = @"SELECT TOP 1 
                    id_versao as IdVersao, 
                    versao as Versao, 
                    data_release as DataRelease, 
                    url_download as UrlDownload, 
                    notas_versao as NotasVersao 
                   FROM Versoes_Software 
                   ORDER BY data_release DESC";

            return db.QueryFirstOrDefault<VersaoSoftware>(sql);
        }

        public bool VerificarSeVersaoEAtual(string versaoAtualApp)
        {
            var ultima = ObterUltimaVersao();
            if (ultima == null) return true; // Se não houver dados, assume que está ok
            return ultima.Versao == versaoAtualApp;
        }
    }
}
