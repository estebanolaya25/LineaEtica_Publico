using CapaDTO.Empresa;

namespace CapaNegocios.Interfaz.empresa.interfaz;

public interface IEmpresaCapaNegocios
{
    Task<List<EmpresaDto>> ListarEmpresas();
}
