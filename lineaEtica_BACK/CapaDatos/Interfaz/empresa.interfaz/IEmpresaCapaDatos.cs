using CapaDTO.Empresa;

namespace CapaDatos.Interfaz.empresa.interfaz;

public interface IEmpresaCapaDatos
{
    Task<List<EmpresaDto>> ListarEmpresas();
}
