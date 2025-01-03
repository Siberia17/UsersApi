using Microsoft.Data.SqlClient;

namespace UsersApi.Data
{
    public class Conexion
    {
        public static SqlConnection GetConexion()
        {
            string connectionString = "Data Source=DESKTOP-K8DNTUE\\MSSQLSERVER2;Database=SiberiaPets;Persist Security Info=True;User ID=siberia;Password=siberia.01;MultipleActiveResultSets=True;Trusted_Connection=true;TrustServerCertificate=true;";
            return new SqlConnection(connectionString);
        }
    }
}
