using CapaDatos.Interfaz.empresa.interfaz;
using CapaDTO.Empresa;
using CapaNegocios.Interfaz.empresa.interfaz;

namespace CapaNegocios.Implementacion.empresa.Implementacion;

public class clsEmpresaCapaNegocios : IEmpresaCapaNegocios
{
    private readonly IEmpresaCapaDatos _datos;

    public clsEmpresaCapaNegocios(IEmpresaCapaDatos datos)
    {
        _datos = datos;
    }

    public Task<List<EmpresaDto>> ListarEmpresas() => _datos.ListarEmpresas();
}
