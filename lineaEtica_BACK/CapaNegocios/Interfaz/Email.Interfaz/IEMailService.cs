using CapaDto.Common;

namespace CapaNegocios.Interfaz.Email.Interfaz
{
    public interface IEMailService
    {
        Task<string> SendMail(EmailMessageRequestDto emailMessage);
    }
}
