namespace CapaDto.Respuestas
{
    public class AuthResponseDto
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string expiration { get; set; }
        public string user { get; set; }
        public int userId { get; set; }
        public bool actualizarPass { get; set; }
    }
}
