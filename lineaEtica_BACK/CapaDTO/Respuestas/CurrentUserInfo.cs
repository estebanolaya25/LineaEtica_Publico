namespace CapaDto.Respuestas
{
    public class CurrentUserInfo
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Usuario { get; set; }
        public string Rol { get; set; }
        public bool ActualizarPass { get; set; }
    }
}
