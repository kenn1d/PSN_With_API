using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using PSN_API.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Получаем имя XML-файла на основе имени сборки проекта API
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

// Настриваем Serilog логер на логирование контроллеров
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File($"Data/Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
// Указываем что логи должны идти через Serilog
builder.Host.UseSerilog();

// Добавляем контекст данных
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(Config.connection, Config.version));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
