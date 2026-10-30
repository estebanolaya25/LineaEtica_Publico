namespace CapaDto.Peticiones
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public int TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string Email { get; set; }
        public int IdTipoUsuario { get; set; }
        public bool Activo { get; set; }
        public string Lang { get; set; }
        public int? IdEmpresa { get; set; }
    }
}
