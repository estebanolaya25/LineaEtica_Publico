using CapaDTO.Registrador;

namespace CapaDatos.Interfaz.registrador.interfaz;

public interface IRegistradorCapaDatos
{
    Task<BuscarUsuarioResponseDto> BuscarUsuarioPorCorreo(string correo);
    Task<List<DenunciaRegistradorResumenDto>> GetMisDenunciasMontadas(int idRegistrador);
    Task<MontarDenunciaResponseDto> MontarDenuncia(MontarDenunciaRequestDto request, int idRegistrador);
}
