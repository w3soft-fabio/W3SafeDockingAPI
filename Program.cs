using Microsoft.EntityFrameworkCore;
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

// ---- Configuração da API ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Documentação automática da API

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Interface visual em /swagger
}

app.MapControllers();

app.Run();
