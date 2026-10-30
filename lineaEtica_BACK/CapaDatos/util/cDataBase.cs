using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CapaDatos.util
{
    public class cDataBase
    {
        private SqlConnection sqlCnn;
        private readonly IConfiguration _configuration;

        public cDataBase(IConfiguration configuration)
        {
            _configuration = configuration;
            string connString = _configuration.GetConnectionString("SQLConnectionString");
            sqlCnn = new SqlConnection(connString);
        }

        public void conectar()
        {
            if (sqlCnn.State != ConnectionState.Open)
                sqlCnn.Open();
        }

        public void desconectar()
        {
            if (sqlCnn.State != ConnectionState.Closed)
                sqlCnn.Close();
        }

        public DataTable ejecutarConsulta(string txtQuery)
        {
            DataTable dtInformation = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(txtQuery, sqlCnn);
            adapter.SelectCommand.CommandType = CommandType.Text;
            adapter.SelectCommand.CommandTimeout = 3600;
            adapter.Fill(dtInformation);
            return dtInformation;
        }

        public void ejecutarQuery(string txtQuery)
        {
            SqlCommand cmd = new SqlCommand(txtQuery, sqlCnn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 3600;
            cmd.ExecuteNonQuery();
        }

        public DataTable mtdEjecutarConsultaSQL(string strConsulta)
        {
            DataTable dtInfo = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(strConsulta, sqlCnn);
            adapter.SelectCommand.CommandType = CommandType.Text;
            adapter.SelectCommand.CommandTimeout = 3600;
            adapter.Fill(dtInfo);
            return dtInfo;
        }
    }
}
