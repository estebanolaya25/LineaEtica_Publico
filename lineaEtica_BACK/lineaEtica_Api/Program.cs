using lineaEtica_Api.Services;
using CapaDatos.Implementacion.auth.Implementacion;
using CapaDatos.Implementacion.denuncia.Implementacion;
using CapaDatos.Implementacion.empresa.Implementacion;
using CapaDatos.Implementacion.investigador.Implementacion;
using CapaDatos.Implementacion.notificaciones.Implementacion;
using CapaDatos.Implementacion.registrador.Implementacion;
using CapaDatos.Interfaz.auth.interfaz;
using CapaDatos.Interfaz.denuncia.interfaz;
using CapaDatos.Interfaz.empresa.interfaz;
using CapaDatos.Interfaz.investigador.interfaz;
using CapaDatos.Interfaz.notificaciones.interfaz;
using CapaDatos.Interfaz.registrador.interfaz;
using CapaNegocios.Implementacion.auth.Implementacion;
using CapaNegocios.Implementacion.denuncia.Implementacion;
using CapaNegocios.Implementacion.Email.Implementacion;
using CapaNegocios.Implementacion.empresa.Implementacion;
using CapaNegocios.Implementacion.investigador.Implementacion;
using CapaNegocios.Implementacion.notificaciones.Implementacion;
using CapaNegocios.Implementacion.registrador.Implementacion;
using CapaNegocios.Interfaz.auth.interfaz;
using CapaNegocios.Interfaz.denuncia.interfaz;
using CapaNegocios.Interfaz.Email.Interfaz;
using CapaNegocios.Interfaz.empresa.interfaz;
using CapaNegocios.Interfaz.investigador.interfaz;
using CapaNegocios.Interfaz.notificaciones.interfaz;
using CapaNegocios.Interfaz.registrador.interfaz;
using lineaEtica_Api.jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);
CapaDatos.util.cEncriptacion.Configurar(builder.Configuration["Encriptacion:Key"], builder.Configuration["Encriptacion:IV"]);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Linea Etica API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddSingleton<TokenGenerator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services
    .AddHttpContextAccessor()
    .AddAuthorization()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddSingleton<IBlobService, BlobService>();

builder.Services.AddScoped<IloginCapaDatos, clsLoginCapaDatos>();
builder.Services.AddScoped<IloginCapaNegocios, clsLoginCapaNegocios>();
builder.Services.AddScoped<IEMailService, EMailService>();
builder.Services.AddScoped<IEmailNotificacionesCapaNegocios, EmailNotificacionesCapaNegocios>();
builder.Services.AddScoped<INotificacionesCapaDatos, clsNotificacionesCapaDatos>();
builder.Services.AddScoped<INotificacionesCapaNegocios, clsNotificacionesCapaNegocios>();
builder.Services.AddScoped<IDenunciaCapaDatos, clsDenunciaCapaDatos>();
builder.Services.AddScoped<IDenunciaCapaNegocios, clsDenunciaCapaNegocios>();
builder.Services.AddScoped<IInvestigadorCapaDatos, clsInvestigadorCapaDatos>();
builder.Services.AddScoped<IInvestigadorCapaNegocios, clsInvestigadorCapaNegocios>();
builder.Services.AddScoped<IRegistradorCapaDatos, clsRegistradorCapaDatos>();
builder.Services.AddScoped<IRegistradorCapaNegocios, clsRegistradorCapaNegocios>();
builder.Services.AddScoped<IEmpresaCapaDatos, clsEmpresaCapaDatos>();
builder.Services.AddScoped<IEmpresaCapaNegocios, clsEmpresaCapaNegocios>();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Linea Etica API v1");
        c.DefaultModelsExpandDepth(-1);
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
