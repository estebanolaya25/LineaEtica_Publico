using CapaDatos.Interfaz.empresa.interfaz;
using CapaDTO.Empresa;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CapaDatos.Implementacion.empresa.Implementacion;

public class clsEmpresaCapaDatos : IEmpresaCapaDatos
{
    private readonly IConfiguration _config;

    private string ConnString => _config.GetConnectionString("SQLConnectionString")!;

    public clsEmpresaCapaDatos(IConfiguration config)
    {
        _config = config;
    }

    public async Task<List<EmpresaDto>> ListarEmpresas()
    {
        return await Task.Run(() =>
        {
            var lista = new List<EmpresaDto>();
            using var connection = new SqlConnection(ConnString);
            connection.Open();
            using var cmd = new SqlCommand(
                "SELECT id, nombre, activo FROM gnl_empresa WHERE activo = 1 ORDER BY nombre", connection);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lista.Add(new EmpresaDto
                {
                    Id = rdr.GetInt32(0),
                    Nombre = rdr.GetString(1),
                    Activo = rdr.GetBoolean(2)
                });
            }
            return lista;
        });
    }
}
