using System.Data;

namespace PSICO.Data.Interfaces.DAL
{
    public interface ICapaDatos
    {
        void ChangeConnectionStringPsicoSettings();
        void ChangeConnectionString(int tenantId);
        void ChangeConnectionStringTest(string connectionString, string source);
        string GetConnectionString();
        DataSet GetDataSet(string consulta);
        string ExecTransaction(string query);
    }
}
