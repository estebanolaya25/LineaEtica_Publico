using CapaDTO.Registrador;

namespace CapaNegocios.Interfaz.registrador.interfaz;

public interface IRegistradorCapaNegocios
{
    Task<BuscarUsuarioResponseDto> BuscarUsuarioPorCorreo(string correo);
    Task<List<DenunciaRegistradorResumenDto>> GetMisDenunciasMontadas(int idRegistrador);
    Task<MontarDenunciaResponseDto> MontarDenuncia(MontarDenunciaRequestDto request, int idRegistrador);
}
