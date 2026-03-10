using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebSafeDockingAPI.Data;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuração do Banco de Dados (MySQL) ----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ---- Repository e Service para Ship ----
builder.Services.AddScoped<IShipRepository, ShipRepository>();
builder.Services.AddScoped<ShipService>();

// ---- Repository e Service para BerthingAlarm ----
builder.Services.AddScoped<IBerthingAlarmRepository, BerthingAlarmRepository>();
builder.Services.AddScoped<BerthingAlarmService>();

// ---- Repository e Service para DriftingAlarm ----
builder.Services.AddScoped<IDriftingAlarmRepository, DriftingAlarmRepository>();
builder.Services.AddScoped<DriftingAlarmService>();

// ---- Repository e Service para MooringPattern ----
builder.Services.AddScoped<IMooringPatternRepository, MooringPatternRepository>();
builder.Services.AddScoped<MooringPatternService>();

// ---- Repository e Service para AlarmThreshold ----
builder.Services.AddScoped<IAlarmThresholdRepository, AlarmThresholdRepository>();
builder.Services.AddScoped<AlarmThresholdService>();

// ---- Repository e Service para Usuario ----
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();

// ---- Repository e Service para Autenticação (JWT) ----
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<PasswordHasherService>();
builder.Services.AddScoped<AuthService>();

// ---- Configuração do Modbus ----
// Mapeia as configurações do appsettings.json para a classe ModbusSettings
builder.Services.Configure<ModbusSettings>(
    builder.Configuration.GetSection("ModbusSettings"));

// ---- Registro dos Serviços ----
// Singleton = uma única instância para toda a aplicação
// Usando FakeModbusConnectionService para simulação (sem sensor real)
builder.Services.AddSingleton<IModbusConnectionService, FakeModbusConnectionService>();
builder.Services.AddSingleton<IModbusReaderService, ModbusReaderService>();

// Registra o serviço de polling que lê dados a cada segundo
builder.Services.AddHostedService<ModbusPollingService>();

// ---- Configuração de Autenticação JWT ----
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Chave JWT não configurada no appsettings.json (Jwt:Key)");

builder.Services.AddAuthentication(options =>
{
    // Define JWT como o esquema padrão de autenticação
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Em desenvolvimento, permite HTTP; em produção, exige HTTPS
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Valida a assinatura do token (garante que não foi alterado)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        // Valida quem emitiu o token
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        // Valida para quem o token foi emitido
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        // Valida se o token não expirou
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Sem tolerância de tempo extra
    };
});

builder.Services.AddAuthorization();

// ---- Configuração da API ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Configura o botão "Authorize" no Swagger para JWT Bearer.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe o token JWT no formato: Bearer {seu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
}); // Documentação automática da API

// ---- CORS (permite Flutter e qualquer frontend acessar a API) ----
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ---- Pipeline HTTP ----
app.UseCors();

// Autenticação e Autorização JWT (a ordem importa!)
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Interface visual em /swagger
}

app.MapControllers();

app.Run();
