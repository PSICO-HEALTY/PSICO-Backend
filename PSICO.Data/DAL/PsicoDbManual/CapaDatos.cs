using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PSICO.Data.Common;
using PSICO.Data.Interfaces.DAL;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.SqlClient;

namespace PSICO.Data.DAL
{
    public class CapaDatos : ICapaDatos
    {
        private readonly string _psicoSettingsConnectionString;
        private string _connectionString;
        private string _sourceDb;
        private readonly IMemoryCache _cache;

        public CapaDatos(
            IOptions<GlobalParams> options,
            IMemoryCache cache)
        {
            _psicoSettingsConnectionString = options.Value.PsicoSettingsConnectionString;
            _cache = cache;
        }

        public void ChangeConnectionStringPsicoSettings()
        {
            _connectionString = _psicoSettingsConnectionString;
            _sourceDb = "sqlsource";
        }

        public void ChangeConnectionString(int tenantId)
        {
            // Obtiene el cache de la data de Entidades
            var entidades = (List<EntidadesData>) _cache.Get("Entidades");

            var companyData = entidades.FirstOrDefault(x => x.Id == tenantId);

            _connectionString = companyData.Psico_Connection_String;
            _sourceDb = companyData.Source;
        }

        public void ChangeConnectionStringTest(string connectionString, string source)
        {
            _connectionString = connectionString;
            _sourceDb = source;
        }

        private DataSet RetornaDataSet(string consulta)
        {
            DataSet dataSet = new DataSet();

            if (_sourceDb.Equals("oraclesource"))
            {
                OracleConnection oracleConnection = new OracleConnection(_connectionString);
                oracleConnection.Open();
                OracleCommand selectCommand = new OracleCommand(consulta, oracleConnection);
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(selectCommand);
                oracleDataAdapter.Fill(dataSet);
                oracleConnection.Close();
                oracleConnection.Dispose();
            }
            else if (_sourceDb.Equals("sqlsource"))
            {
                using SqlConnection sqlConnection = new SqlConnection(_connectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(consulta, sqlConnection);
                sqlCommand.CommandTimeout = 0;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dataSet);
                sqlCommand.Dispose();
                sqlDataAdapter.Dispose();
                sqlConnection.Close();
                sqlConnection.Dispose();
            }

            return dataSet;
        }

        private string Acciones(string query)
        {
            try
            {
                if (_sourceDb.Equals("oraclesource"))
                {
                    OracleConnection oracleConnection = new OracleConnection(_connectionString);
                    oracleConnection.Open();
                    OracleCommand oracleCommand = oracleConnection.CreateCommand();
                    oracleCommand.CommandText = query;
                    oracleCommand.ExecuteNonQuery();
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
                else if (_sourceDb.Equals("sqlsource"))
                {
                    using SqlConnection sqlConnection = new SqlConnection(_connectionString);
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }

                return "SUCCESS";
            }

            catch (Exception ex)
            {
                string text2 = "";
                string text3 = "";
                try
                {
                    text2 = AppDomain.CurrentDomain.BaseDirectory;
                    if (!Directory.Exists(text2))
                    {
                        Directory.CreateDirectory(text2);
                    }

                    text3 = text2 + "Log(" + DateTime.Now.ToString("dd-MM-yyyy") + ").txt";
                    StreamWriter streamWriter = new StreamWriter(text3, append: true);
                    streamWriter.WriteLine(string.Concat("* ", DateTime.Now, " || Query: ", query, " - ", ex.Message.ToString()));
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                catch (Exception e) { }

                return ex.Message.ToString();
            }
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public DataSet GetDataSet(string consulta)
        {
            return RetornaDataSet(consulta);
        }

        public string ExecTransaction(string query)
        {
            return Acciones(query);
        }
    }
}