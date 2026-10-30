namespace CapaNegocios.Implementacion.Email.Implementacion;

public static class EmailTemplateHelper
{
    private const string Accent     = "#486581";
    private const string AccentDark = "#334e68";
    private const string TextColor  = "#1a202c";
    private const string MutedColor = "#718096";
    private const string BgColor    = "#f4f6f8";
    private const string BorderColor = "#e9eef4";

    public static string Layout(string titulo, string cuerpoHtml, string urlAmbiente, string? urlBoton = null, string? textoBoton = null)
    {
        string boton = "";
        if (!string.IsNullOrWhiteSpace(urlBoton))
        {
            boton = $@"
            <table role='presentation' cellpadding='0' cellspacing='0' style='margin-top:24px;'>
              <tr>
                <td style='border-radius:6px;background-color:{Accent};'>
                  <a href='{urlBoton}' target='_blank'
                     style='display:inline-block;padding:12px 28px;font-family:Segoe UI,Arial,sans-serif;font-size:14px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:6px;'>
                    {textoBoton ?? "Ir al aplicativo"}
                  </a>
                </td>
              </tr>
            </table>";
        }

        return $@"<!DOCTYPE html>
<html lang='es'>
<body style='margin:0;padding:0;background-color:{BgColor};'>
  <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background-color:{BgColor};padding:32px 16px;font-family:Segoe UI,Arial,sans-serif;'>
    <tr>
      <td align='center'>
        <table role='presentation' width='600' cellpadding='0' cellspacing='0' style='max-width:600px;width:100%;background-color:#ffffff;border-radius:10px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,0.06);'>
          <tr>
            <td style='background-color:{Accent};background-image:linear-gradient(135deg,{Accent},{AccentDark});padding:22px 32px;'>
              <span style='font-family:Segoe UI,Arial,sans-serif;font-size:17px;font-weight:600;color:#ffffff;letter-spacing:.2px;'>
                Canal Ético Corporativo
              </span>
            </td>
          </tr>
          <tr>
            <td style='padding:32px;'>
              <h2 style='margin:0 0 16px;font-family:Segoe UI,Arial,sans-serif;font-size:19px;color:{TextColor};'>{titulo}</h2>
              <div style='font-family:Segoe UI,Arial,sans-serif;font-size:14.5px;line-height:1.65;color:{TextColor};'>
                {cuerpoHtml}
              </div>
              {boton}
            </td>
          </tr>
          <tr>
            <td style='padding:18px 32px;background-color:{BgColor};border-top:1px solid {BorderColor};'>
              <p style='margin:0 0 4px;font-family:Segoe UI,Arial,sans-serif;font-size:12px;color:{MutedColor};'>
                Este es un mensaje automático del Canal Ético Corporativo. Por favor no responda a este correo.
              </p>
              <p style='margin:0;font-family:Segoe UI,Arial,sans-serif;font-size:12px;color:{MutedColor};'>
                {urlAmbiente}
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public static string CajaDestacada(string etiqueta, string valor)
    {
        return $@"
        <table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='margin:16px 0;'>
          <tr>
            <td style='background-color:{BgColor};border-left:4px solid {Accent};border-radius:6px;padding:12px 18px;'>
              <span style='display:block;font-family:Segoe UI,Arial,sans-serif;font-size:11px;font-weight:600;color:{MutedColor};text-transform:uppercase;letter-spacing:.06em;'>{etiqueta}</span>
              <span style='display:block;font-family:Segoe UI,Arial,sans-serif;font-size:20px;font-weight:700;color:{TextColor};margin-top:2px;'>{valor}</span>
            </td>
          </tr>
        </table>";
    }

    public static string CajaCredenciales(string usuario, string password)
    {
        return $@"
        <table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='margin:16px 0;'>
          <tr>
            <td style='background-color:{BgColor};border-left:4px solid {Accent};border-radius:6px;padding:14px 18px;'>
              <span style='display:block;font-family:Segoe UI,Arial,sans-serif;font-size:11px;font-weight:600;color:{MutedColor};text-transform:uppercase;letter-spacing:.06em;'>Usuario</span>
              <span style='display:block;font-family:Segoe UI,Arial,sans-serif;font-size:15px;color:{TextColor};margin:2px 0 10px;'>{usuario}</span>
              <span style='display:block;font-family:Segoe UI,Arial,sans-serif;font-size:11px;font-weight:600;color:{MutedColor};text-transform:uppercase;letter-spacing:.06em;'>Contraseña</span>
              <span style='display:block;font-family:Consolas,Courier New,monospace;font-size:17px;font-weight:700;color:{TextColor};margin-top:2px;letter-spacing:.03em;'>{password}</span>
            </td>
          </tr>
        </table>";
    }

    public static string Parrafo(string texto) =>
        $"<p style='margin:0 0 12px;'>{texto}</p>";
}
