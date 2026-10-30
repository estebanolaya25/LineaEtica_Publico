namespace CapaDto.Peticiones
{
    public class UsuariolistDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Identificacion { get; set; }
        public int TipoIdentificacion { get; set; }
        public string Email { get; set; }
        public string Usuario { get; set; }
        public int IdTipoUsuario { get; set; }
        public string NombreTipoUsuario { get; set; }
        public bool Activo { get; set; }
        public int? IdEmpresa { get; set; }
        public string? NombreEmpresa { get; set; }
    }
}
