using CapaDto.Peticiones;
using CapaDto.Respuestas;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;

namespace lineaEtica_Api.jwt
{
    public class TokenGenerator
    {
        private readonly IConfiguration _configuration;

        public TokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthResponseDto GenerateTokenJwt(LoginRequest Userinfo, DataTable userinfo)
        {
            int userId  = Convert.ToInt32(userinfo.Rows[0]["Id"]);
            string rol  = userinfo.Rows[0]["Rol"].ToString();
            bool actualizarPass = userinfo.Columns.Contains("ActualizarPass") &&
                                  userinfo.Rows[0]["ActualizarPass"] != DBNull.Value &&
                                  Convert.ToBoolean(userinfo.Rows[0]["ActualizarPass"]);
            int? idEmpresa = userinfo.Columns.Contains("IdEmpresa") &&
                             userinfo.Rows[0]["IdEmpresa"] != DBNull.Value
                             ? Convert.ToInt32(userinfo.Rows[0]["IdEmpresa"])
                             : null;
            string? nombreEmpresa = userinfo.Columns.Contains("NombreEmpresa") &&
                             userinfo.Rows[0]["NombreEmpresa"] != DBNull.Value
                             ? userinfo.Rows[0]["NombreEmpresa"].ToString()
                             : null;
            string jwtToken    = GenerarAccessToken(Userinfo.email, rol, userId, idEmpresa, nombreEmpresa);
            string refreshToken = GenerarRefreshToken();

            return new AuthResponseDto
            {
                access_token   = jwtToken,
                refresh_token  = refreshToken,
                expiration     = _configuration["Jwt:JWT_EXPIRE_MINUTES"],
                user           = Userinfo.email,
                userId         = userId,
                actualizarPass = actualizarPass
            };
        }

        public string GenerarAccessToken(string email, string rol, int userId = 0, int? idEmpresa = null, string? nombreEmpresa = null)
        {
            var secretKey     = _configuration["Jwt:Key"];
            var audienceToken = _configuration["Jwt:Audience"];
            var issuerToken   = _configuration["Jwt:Issuer"];
            int expireMinutes = int.TryParse(_configuration["Jwt:JWT_EXPIRE_MINUTES"], out int mins) && mins > 0
                                ? mins : 60;

            var securityKey        = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("name",   email),
                new Claim("sub",    email),
                new Claim("email",  email),
                new Claim("role",   rol),
                new Claim("userId", userId.ToString())
            };
            if (idEmpresa.HasValue)
                claims.Add(new Claim("idEmpresa", idEmpresa.Value.ToString()));
            if (!string.IsNullOrWhiteSpace(nombreEmpresa))
                claims.Add(new Claim("nombreEmpresa", nombreEmpresa));

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.CreateJwtSecurityToken(
                audience: audienceToken,
                issuer: issuerToken,
                subject: claimsIdentity,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: signingCredentials);

            return tokenHandler.WriteToken(jwtSecurityToken);
        }

        public string GenerarRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public DateTime ObtenerExpiracionRefreshToken()
        {
            int diasExpiracion = int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int dias) ? dias : 7;
            return DateTime.UtcNow.AddDays(diasExpiracion);
        }
    }
}
